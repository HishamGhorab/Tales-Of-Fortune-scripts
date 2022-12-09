using System;
using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

public class TOFRoundState : MonoBehaviour
{ 
    public enum RoundState {PlanningPhase, CashingPhase, PlayingPhase}

    public static int CurrentRound = 0;
    
    [Header("Debugging")]
    public RoundState roundState;
    public int currentSegment = 0;

    [Header("Components")]
    //public StateTimesScriptableObject stateTimesSO;
    public TOFTimeHandler timeHanlder;
    public TOFGameController gameController;

    public float timeToSwap;

    public bool roundSwapped = false;
    
    public delegate void OnPlanningStart();
    public delegate void OnPlayingStart();
    public delegate void OnCashingStart();

    public OnPlanningStart onPlanningStart;
    public OnPlanningStart onPlayingStart;
    public OnCashingStart onCashingStart;

    private bool startSegment = true;
    private bool startCashing = true;
    private bool startPlanning = true;

    public delegate void OnSegmentStart(int segment);
    public static OnSegmentStart onSegmentStart;
    
    public float segmentSwapTime;

    private static TOFRoundState _singleton;
    public static TOFRoundState Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(TOFRoundState)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void OnEnable()
    {
        onPlanningStart += SetRoundState;
        onPlayingStart += SetRoundState;
        onSegmentStart += SendSegmentMoves;
    }

    private void OnDisable()
    {
        onPlanningStart -= SetRoundState;
        onPlayingStart -= SetRoundState;
        onSegmentStart -= SendSegmentMoves;
    }

    private void Awake()
    {
        Singleton = this;   
    }

    private void Start()
    {
        roundState = RoundState.PlanningPhase;
        SetNewSwapTime(TOFGameMode.Singleton.planningRoundLength);
    }

    void Update()
    {
        switch (roundState)
        {
            case RoundState.PlanningPhase: PlanningPhase();
                break;
            case RoundState.CashingPhase: CashingPhase();
                break;
            case RoundState.PlayingPhase: PlayingPhase();
                break;
        }
        
        void PlanningPhase()
        {
            if (startPlanning)
            {
                //TODO: remove all clients that have sinking true? XDDDDD
                foreach (TOFPlayer p in TOFPlayer.players.Values)
                {
                    if (!p.isAlive)
                        TOFPlayer.players.Remove(p.id);
                }
                
                currentSegment = 0;
                CurrentRound++;
            
                SetNewSwapTime(TOFGameMode.Singleton.planningRoundLength);
                onPlanningStart.Invoke();

                startPlanning = false;
            }
            
            if (timeHanlder.GameTime >= timeToSwap)
            {
                startCashing = true;
                roundState = RoundState.CashingPhase;
            }
        }
        
        void CashingPhase()
        {
            //todo: Store moves here instead and give a margin for the players to send the moves (if lagging)
            if (startCashing)
            {
                TOFGameController.Singleton.ClearLists();
                
                onCashingStart.Invoke();
                StartCoroutine(StartPlayingAfterSeconds(1));
                
                //todo fix a way to go to playing faster if players are ready
                
                startCashing = false;
            }

            IEnumerator StartPlayingAfterSeconds(int seconds)
            {
                yield return new WaitForSeconds(seconds);
                roundState = RoundState.PlayingPhase;
            }
        }
        
        void PlayingPhase()
        {
            if (startSegment)
            {
                //Clearing the segment time before setting its value again.
                TOFTimeHandler.Singleton.ClearSegmentTime();
                
                for (int i = 0; i < TOFGameController.allClients.Count; i++)
                {
                    TOFTimeHandler.Singleton.UpdateCurrentSegmentTime(i, currentSegment);
                }
                
                onSegmentStart.Invoke(currentSegment);
                
                segmentSwapTime = TOFTimeHandler.Singleton.GameTime + GetCurrentSegmentTime();
                
                //Debug.Log("Segment length: " + segmentSwapTime);

                startSegment = false;
                
                //todo: find more suitable way to write this
                foreach (TOFPlayer player in TOFPlayer.players.Values)
                {
                    player.sinking = false;
                }
            }
            
            if (TOFTimeHandler.Singleton.GameTime >= segmentSwapTime)
            {
                currentSegment++;
                SendSegmentToClients(currentSegment);

                startSegment = true;
                
                if (currentSegment > 3)
                {
                    startPlanning = true;
                    roundState = RoundState.PlanningPhase;
                }
            }
        }
    }
    
    public int GetCurrentSegmentTime()
    {
        return TOFTimeHandler.Singleton.segmentTime;
    }

    public void SetNewSwapTime(float swapTime)
    {
        timeToSwap = Mathf.Round(swapTime + timeHanlder.GameTime);
    }

    private void SetRoundState()
    {
        Message _message = Message.Create(MessageSendMode.reliable, ServerToClientId.sendRoundState);

        _message.AddString(roundState.ToString());
        NetworkManager.Singleton.Server.SendToAll(_message);
    }

    void SendSegmentMoves(int segment)
    {
        //Debug.Log("Index: "+ segment + " at: " + TOFTimeHandler.Singleton.GameTime);
        TOFGameController.Singleton.SendSegmentMovesToClient(segment);
    }
    
    void SendSegmentToClients(int segment)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.sendSegmentToClient);
        message.AddInt(segment);
        
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
