using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
public enum DynamicPiece {Empty, Ship};
public enum StaticPiece {Empty, Wave, Whirlpool};

public class TOFState
{
    private static TOFState _singleton;
    public static TOFState Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(TOFGameModel)} instance already exists, destroying duplicate!");
            }
        }
    }

    public TOFState()
    {
        Singleton = this;
    }

    public DynamicPiece[,] dynamicPieceBoard;
    public StaticPiece[,] staticPieceBoard;

    //todo: depricate
    public int[] CloneBoard()
    {
        int sizeX = TOFGameMode.Singleton.boardSize.x;
        int sizeY = TOFGameMode.Singleton.boardSize.y;
        
        int[] board = new int[sizeX * sizeY]; //16*16
        
        for (int y = 0; y < sizeY; y++) //0 - 15
        {
            for (int x = 0; x < sizeX; x++) //16*16
            {
                //board[Get2DPositionInArray(x,y)] = Convert.ToInt32(pieceBoard[x,y]); //instead ship it says 1
                Debug.Log(Get2DPositionInArray(x,y));
            }
        }
        return board;
    }

    public static int Get2DPositionInArray(int x, int y)
    {
        int sizeX = TOFGameMode.Singleton.boardSize.x;
        return (((y * sizeX) - sizeX) + x) - 1;
    }
}
