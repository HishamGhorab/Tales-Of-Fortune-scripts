using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TOFGameMode : MonoBehaviour
{
    //make this into a scriptableObject?
    
    private static TOFGameMode _singleton;
    public static TOFGameMode Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(TOFGameMode)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }
    
    //THESE VARIABLES SHOULD BE CHANGED THROUGH THE INSPECTOR!!!
    [Header("Start Variables")]
    public Vector2Int[] playerStartPositions = { new Vector2Int(5, 6), new Vector2Int(3, 1), new Vector2Int(3, 2)};
    public int[] playerStartRotations = {0, 0, 0};
    
    //THESE VARIABLES SHOULD BE CHANGED THROUGH THE INSPECTOR!!!
    [Header("Board Variables")]
    public Vector2Int boardSize = new Vector2Int(16, 16);

    //THESE VARIABLES SHOULD BE CHANGED THROUGH THE INSPECTOR!!!

    [Header("Move Timers")]
    public int moveForwardTime;
    public int moveRightTime;
    public int moveCannonTime;
    public int sinkTime;

    //THESE VARIABLES SHOULD BE CHANGED THROUGH THE INSPECTOR!!!
    [Header("Round Variables")]
    public int planningRoundLength;
    public int segmentsInRound;

    public int playingRoundLength;

    //temporary
    public int maxHealth;

    private void Awake()
    {
        Singleton = this;
        
    }
}
