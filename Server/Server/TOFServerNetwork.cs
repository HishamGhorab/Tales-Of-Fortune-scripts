using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TOFServerNetwork : MonoBehaviour
{
    public delegate int[] OnFetchMoves();
    public OnFetchMoves onFetchMoves;
    
    public int numPlayers;

    //private static List<TOFClient> clients = new List<TOFClient>();
    private List<int[]> playersMoves = new List<int[]>();

    private TOFRoundState tofRoundState;

    void Awake()
    {
        InitClients(numPlayers);
        tofRoundState = GetComponent<TOFRoundState>();
    }

    private void Update()
    {
        if (tofRoundState.roundSwapped && tofRoundState.roundState == TOFRoundState.RoundState.PlayingPhase)
        {
            FetchMovesFromClients();
            tofRoundState.roundSwapped = false;
        }
    }

    private void FetchMovesFromClients() // Invokes event that returns all moves from players and caches them
    {
        if (onFetchMoves != null)
        {
            foreach (Delegate d in onFetchMoves.GetInvocationList())
            {
                playersMoves.Add((int[])d.DynamicInvoke());
            }
                
            //playersMoves.Clear(); // Temp clearing
        }
    }

    public void InitClients(int numPlayers)
    {
        for (int i = 0; i < numPlayers; i++)
        {
            GameObject go = new GameObject();
            go.name = $"Client {i + 1}";
            //go.AddComponent<TOFPlayerController>();
            //TOFClient client = go.AddComponent<TOFClient>();
            //client.id = i;
            //clients.Add(client);
        }
    }

    public void SendRoundState(TOFRoundState roundState)
    {
        //todo
    }
}
