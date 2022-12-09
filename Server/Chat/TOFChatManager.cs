using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

public class TOFChatManager : MonoBehaviour
{
    private static TOFChatManager _singleton;
    public static TOFChatManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(TOFChatManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }
    
    private static int chatLength = 0;
    [MessageHandler((ushort)ClientToServerId.sendChatText)]
    private static void UpdateChat(ushort fromClientId, Message message)
    {
        string text = message.GetString();
        string timeText = "[" + TOFTimeHandler.GetTimeAsMinutes() + "]";

        //string newText = $"{timeText} {TOFPlayer.players[fromClientId].username}: {text}";

        string time = timeText;
        string username = TOFPlayer.players[fromClientId].username;
        string chatMessage = text;
        
        Message _message = Message.Create(MessageSendMode.reliable, ServerToClientId.sendChatText);
        _message.AddUShort(fromClientId);
        _message.AddString(time);
        _message.AddString(username);
        _message.AddString(chatMessage);
        
        NetworkManager.Singleton.Server.SendToAll(_message);
        
        chatLength++;
    }

    public static void SendServerChatMessage(string chatMessage, ushort client)
    {
        string timeText = "[" + TOFTimeHandler.GetTimeAsMinutes() + "]";
        
        Message _message = Message.Create(MessageSendMode.reliable, ServerToClientId.sendServerChatText);
        _message.AddString(timeText);
        _message.AddString(chatMessage);

        NetworkManager.Singleton.Server.Send(_message, client);
        chatLength++;
    }
    
    public static void SendServerChatMessage(string chatMessage)
    {
        string timeText = "[" + TOFTimeHandler.GetTimeAsMinutes() + "]";
        
        Message _message = Message.Create(MessageSendMode.reliable, ServerToClientId.sendServerChatText);
        _message.AddString(timeText);
        _message.AddString(chatMessage);

        NetworkManager.Singleton.Server.SendToAll(_message);
        chatLength++;
    }

    [MessageHandler((ushort) ClientToServerId.gameSceneLoaded)]
    static void GameSceneLoaded(ushort fromClientId, Message message)
    {
        string chatMessage = "Game session has successfully started.";
        SendServerChatMessage(chatMessage, fromClientId);
    }
}
