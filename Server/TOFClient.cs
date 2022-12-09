using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

public class TOFClient
{
    public static Dictionary<ushort, TOFClient> clients = new Dictionary<ushort, TOFClient>();

    public ushort Id { get; set; }
    public string Username { get; set; }
    
    #region Messages
    public void SendMatchJoined()
    {
        NetworkManager.Singleton.Server.SendToAll(MakeMatchedMessage(Message.Create(MessageSendMode.reliable, ServerToClientId.clientMatched)));
    }
    
    public void SendMatchJoined(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(MakeMatchedMessage(Message.Create(MessageSendMode.reliable, ServerToClientId.clientMatched)), toClientId);
    }
    
    public void SendMatchLeft()
    {
        NetworkManager.Singleton.Server.SendToAll(MakeMatchedMessage(Message.Create(MessageSendMode.reliable, ServerToClientId.clientLeft)));
    }
    
    public void SendMatchLeft(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(MakeMatchedMessage(Message.Create(MessageSendMode.reliable, ServerToClientId.clientLeft)), toClientId);
    }

    private Message MakeMatchedMessage(Message message) //todo: REFACTORTHISSHIT
    {
        message.AddUShort(Id);
        message.AddString(Username);
        return message;
    }
    
    #endregion
}
