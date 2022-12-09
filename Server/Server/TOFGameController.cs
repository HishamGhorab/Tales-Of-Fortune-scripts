using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RiptideNetworking;
using UnityEngine;

public class TOFGameController : MonoBehaviour
{
    public static Dictionary<ushort, int[]> eachPlayerMoves = new Dictionary<ushort, int[]>();
    public static Dictionary<ushort, bool[]> rightCannons = new Dictionary<ushort, bool[]>();
    public static Dictionary<ushort, bool[]> leftCannons = new Dictionary<ushort, bool[]>();
    
    public static List<ushort> allClients;
    public static List<int[]> allmoves;
    
    private static TOFGameController _singleton;
    public static TOFGameController Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(TOFGameController)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public void ClearLists()
    {
        eachPlayerMoves.Clear();
        rightCannons.Clear();
        leftCannons.Clear();
        
        allmoves.Clear();
        allClients.Clear();
    }

    private void OnEnable()
    {
        GetComponent<TOFRoundState>().onCashingStart += FetchMoves;
    }

    private void OnDisable()
    {
        GetComponent<TOFRoundState>().onCashingStart -= FetchMoves;
    }

    private void Awake()
    {
        Singleton = this;
        
        allClients = new List<ushort>();
        allmoves = new List<int[]>();
    }

    public void FetchMoves()
    {
        Message _message = Message.Create(MessageSendMode.reliable, ServerToClientId.onFetchMoves);
        //todo: add that its playing phase
        NetworkManager.Singleton.Server.SendToAll(_message);
    }
    
    //Make combine moves into a server controlled method. When server calls for fetch moves after X seconds call this method
    [MessageHandler((ushort) ClientToServerId.sendPlayerMoves)]
    static void StoreRoundMoves(ushort clientId, Message message)
    {
        //TODO: this method can break pretty easily. Considering some shit

        eachPlayerMoves[clientId] = message.GetInts();

        rightCannons[clientId] = message.GetBools();
        leftCannons[clientId] = message.GetBools();

        allClients.Add(clientId);
        allmoves.Add(eachPlayerMoves[clientId]);
    }

    public void SendSegmentMovesToClient(int moveIndex)
    {
        Message _message = Message.Create(MessageSendMode.reliable, ServerToClientId.onSendMovesBackToClient);

        List<string> playerPositions = new List<string>();
        ushort[] clientsHit = new ushort[allClients.Count];
        
        //Movement
        for (int i = 0; i < allClients.Count; i++)
        {
            ushort client = allClients[(ushort)i];
            TOFPlayer player = TOFPlayer.players[client];
            
            bool rightCannon = rightCannons[client][moveIndex];
            bool leftCannon = leftCannons[client][moveIndex];
            
            //if player is dead

            Vector2 startPos = player.position;
            if (!TOFPlayer.players[client].isAlive)
            {
                TOFGameModel.Singleton.SetShipPosAndRot(client, 4);
            }
            else
            {
                TOFGameModel.Singleton.SetShipPosAndRot(client, allmoves[i][moveIndex]); 
            }
            Vector2 endPos = player.position;

            //Debug.Log($"Move {move}, Client:{client}, StartPos:{startPos}, EndPos:{endPos}, rotation {TOFPlayer.players[client].rotation}");
            //Debug.Log($"{client}_{startPos.x}_{startPos.y}_{endPos.x}_{endPos.y}_{TOFPlayer.players[client].rotation}");
            playerPositions.Add($"{client}_{allmoves[i][moveIndex]}_{startPos.x}_{startPos.y}_{endPos.x}_{endPos.y}_{TOFPlayer.players[client].rotation}_{rightCannon}_{leftCannon}");
        }
        
        //Shooting
        for (int i = 0; i < allClients.Count; i++)
        {
            ushort client = allClients[(ushort)i];
            TOFPlayer player = TOFPlayer.players[client];
            
            bool rightCannon = rightCannons[client][moveIndex];
            bool leftCannon = leftCannons[client][moveIndex];
            
            //Set cannons shots. What we are sending is the player that got hit by this ship
            Vector2Int hitPosition = new Vector2Int();
            ushort hitClient = ushort.MaxValue;

            if (TOFGameModel.Singleton.ProjectileHitCheck(client, player.rotation, rightCannon, leftCannon, out hitPosition))
            {
                Debug.Log("Cannon hit");
                TOFPlayer damagedPlayer = TOFGameModel.Singleton.DealDamageToPlayer(hitPosition);
                hitClient = damagedPlayer.id;
            }
            clientsHit[i] = hitClient;
        }

        _message.AddStrings(playerPositions.ToArray());
        _message.AddUShorts(clientsHit);
        
        NetworkManager.Singleton.Server.SendToAll(_message);
    }
}
