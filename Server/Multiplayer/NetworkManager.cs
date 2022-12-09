using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

//General
public enum ServerToClientId : ushort
{
    clientMatched = 1, clientLeft, matchmakingClientsChanged, isLobbyLeader, onSessionStarted, updateSegmentTimes, 
    updateHealthValues, AddPlayerToList,

    //Session flow
    sendSegmentToClient, onFetchMoves, sendRoundState, onSendMovesBackToClient,
    
    //chat
    sendChatText, sendServerChatText, 
}

public enum ClientToServerId : ushort
{
    onConnectedClientData = 1, onDisconnectedClientData, onSessionStart,

    //Session flow
    sendPlayerMoves, gameSceneLoaded,
    
    //chat
    sendChatText,
}

public class NetworkManager : MonoBehaviour
{
    public static bool sessionStarted = false;
    
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }
    
    public Server Server { get; private set; }
    
    //Use the UShort data type to contain binary data too large for Byte
    [SerializeField] public ushort port;
    [Range(1, 10)][SerializeField] public ushort maxClientCount;
    
    private void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        Application.targetFrameRate = 60;
        
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        Server = new Server();
        Server.Start(port, maxClientCount);
        //Server.ClientDisconnected += PlayerLeft;
        Server.ClientDisconnected += ClientLeft;
    }

    private void FixedUpdate()
    {
        Server.Tick();
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
    }
    
    private void ClientLeft(object sender, ClientDisconnectedEventArgs e)
    {
        TOFClient.clients.Remove(e.Id);
    }
}
