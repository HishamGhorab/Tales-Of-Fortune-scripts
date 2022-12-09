using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using RiptideNetworking;

public class TOFMatchmakingManager : MonoBehaviour
{
    private int numOfClients = 0;

    private void Update()
    {
        if (numOfClients != TOFClient.clients.Count)
        {
            //update matchmaking ui
            Message message = Message.Create(MessageSendMode.reliable, (ushort) ServerToClientId.matchmakingClientsChanged);
            
            message.AddInt(TOFClient.clients.Count);
            message.AddUShort(NetworkManager.Singleton.maxClientCount);
            NetworkManager.Singleton.Server.SendToAll(message);

            numOfClients = TOFClient.clients.Count;
        }
    }

    public static void AddClientToMatch(ushort id, string username)
    {
        foreach (TOFClient otherMatchedClient in TOFClient.clients.Values)
        {
            //send to myself all that have joined
            otherMatchedClient.SendMatchJoined(id);
        }

        TOFClient client = new TOFClient();
        client.Id = id;
        client.Username = string.IsNullOrEmpty(username) ? $"Guest{id}" : username;
        
        TOFClient.clients.Add(id, client);
        
        //send to all that i have joined!
        client.SendMatchJoined();

        SendLobbyLeaderMessage();
    }

    public static void RemoveClientFromMatch(ushort id, string username)
    {
        //send to myself that ive removed myself
        TOFClient.clients[id].SendMatchLeft(id);
        //send to all that i have removed myself
        TOFClient.clients[id].SendMatchLeft();
        
        //remove myself from the server
        TOFClient.clients.Remove(id);
        
        SendLobbyLeaderMessage();
    }
    
    private static void SendLobbyLeaderMessage()
    {
        //send lobby leader message and ability to start game.
        Message _message = Message.Create(MessageSendMode.reliable, ServerToClientId.isLobbyLeader);

        // ReSharper disable once ReplaceWithSingleAssignment.True
        bool canStart = true;
        if (TOFClient.clients.Count < NetworkManager.Singleton.maxClientCount)
            canStart = false;
        

        _message.AddBool(canStart);
        NetworkManager.Singleton.Server.Send(_message, 1);
    }

    #region MessageHandlers
    [MessageHandler((ushort) ClientToServerId.onConnectedClientData)]
    private static void OnConnectedClientData(ushort fromClientId, Message message)
    {
        AddClientToMatch(fromClientId, message.GetString());
    }
    
    
    [MessageHandler((ushort) ClientToServerId.onDisconnectedClientData)]
    private static void OnDisconnectedClientData(ushort fromClientId, Message message)
    {
        RemoveClientFromMatch(fromClientId, message.GetString());
    }

    [MessageHandler((ushort) ClientToServerId.onSessionStart)]
    private static void StartSession(ushort fromClientId, Message message)
    {
        Debug.Log("Session started");
        //todo : better system that accounts for loading time.
        NetworkManager.sessionStarted = true;
        
        //todo: create board in the model
        TOFGameModel.Singleton.StartGame();

        //send message to every client to start their game
        Message _message = Message.Create(MessageSendMode.reliable, ServerToClientId.onSessionStarted);

        CreatePlayers();      
        
        //RoundSpecific
        _message.AddInt(TOFGameMode.Singleton.segmentsInRound);
        _message.AddInt(TOFGameMode.Singleton.moveCannonTime);
        
        //todo: look into maxClientCount and see if replaceable
        _message.AddInt(NetworkManager.Singleton.maxClientCount);

        /*for (int i = 0; i < TOFClient.clients.Count; i++)
        {
            _message.AddVector2(TOFGameMode.Singleton.playerStartPositions[i]);
            _message.AddInt(TOFGameMode.Singleton.playerStartRotations[i]);
        }*/

        //todo: Specify scene if needed.
        NetworkManager.Singleton.Server.SendToAll(_message);

        void CreatePlayers()
        {
            int count = 0;
            foreach (var client in TOFClient.clients.Values)
            {
                ushort id = client.Id;
                string username = client.Username;
                
                Vector2Int startPosition = TOFGameMode.Singleton.playerStartPositions[count];
                int startRot = TOFGameMode.Singleton.playerStartRotations[count];
                
                TOFPlayer player = new TOFPlayer(id, startPosition, startRot, TOFGameMode.Singleton.maxHealth);
                TOFPlayer.players.Add(id, player);
                count++;
            }
        }

        void CreateEnemys()
        {
            int playersCount = TOFPlayer.players.Count;
        }
    }
    
    #endregion
}
