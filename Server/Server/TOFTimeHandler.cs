using System;
using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

public class TOFTimeHandler : MonoBehaviour
{
    public int GameTime { get { return (int)_gameTime;} private set { _gameTime = value; } }
    public bool pauseTime;

    public int segmentTime;

    [SerializeField] private float _gameTime;

    private static TOFTimeHandler _singleton;
    public static TOFTimeHandler Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(TOFTimeHandler)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        _gameTime = 0;
    }
    void Update()
    {
        //Only update if the games sessions has started
        if (!NetworkManager.sessionStarted)
            return;
        
        if(!pauseTime)
            UpdateTime();
    }

    void UpdateTime()
    {
        _gameTime += Time.deltaTime;
        
        //SendGameTimeToClient();
    }

    public void PauseTime()
    {
        pauseTime = true;
    }    
    
    public void StarTime()
    {
        pauseTime = false;
    }

    //todo: these needs urget looking into!
    
    /*void SendGameTimeToClient()
    {
        Message _message = Message.Create(MessageSendMode.unreliable, ServerToClientId.sendTimeAndRoundToClient);

        _message.AddInt(GameTime);
        _message.AddInt(TOFRoundState.CurrentRound);
        
        NetworkManager.Singleton.Server.SendToAll(_message);
    }*/
    
    public static string GetTimeAsMinutes()
    {
        string minutes = Mathf.Floor(Singleton.GameTime / 60).ToString("00");
        string seconds = (Singleton.GameTime % 60).ToString("00");
     
        return string.Format("{0}:{1}", minutes, seconds);
    }

    public void UpdateCurrentSegmentTime(int clientIndex, int currentSegment)
    {
        //Debug.Log("Allclients " + TOFGameController.allClients.Count);
        //Debug.Log("allmoves " + TOFGameController.allmoves.Count);
        //Debug.Log("cs " + currentSegment);
        int newTime = 0;
        int moveTime = 0;
        
        ushort client = TOFGameController.allClients[(ushort)clientIndex];
       
        int move = TOFGameController.allmoves[clientIndex][currentSegment];
        bool rightCannon = TOFGameController.rightCannons[client][currentSegment];
        bool leftCannon = TOFGameController.leftCannons[client][currentSegment];

        int cannonShotTime = TOFGameMode.Singleton.moveCannonTime;
        int sinkTime = TOFGameMode.Singleton.sinkTime;



        switch (move)
        {
            case 0: newTime = 0;
                break;
            case 1: newTime = TOFGameMode.Singleton.moveForwardTime;
                break;
            case 2: newTime = TOFGameMode.Singleton.moveRightTime;
                break;
            case 3: newTime = TOFGameMode.Singleton.moveRightTime;
                break;
        }
        
        if(moveTime < newTime) 
            moveTime = newTime;
        
        //increase segment if cannons are shot
        if(rightCannon || leftCannon)
        {
            newTime += cannonShotTime;
        }
        
        //increase segment time if a ship is sinking
        foreach (TOFPlayer player in TOFPlayer.players.Values)
        {
            if (player.sinking)
            {
                newTime += sinkTime;
            }
        }

        if (newTime > segmentTime)
        {
            segmentTime = newTime;
        }

        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.updateSegmentTimes);
        
        message.AddInt(segmentTime);

        message.AddInt(moveTime);
        message.AddInt(cannonShotTime);
        message.AddInt(sinkTime);
        
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void ClearSegmentTime()
    {
        segmentTime = 0;
    }
}
