using System.Collections;
using System.Collections.Generic;
using RiptideNetworking;
using UnityEngine;

public class TOFGameModel : MonoBehaviour
{
    //this is very temporary! This should be handled by network messages
    //public TOFGameView gameView;
    
    private static TOFGameModel _singleton;
    
    private TOFState state;
    
    public static TOFGameModel Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(TOFGameModel)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }
    
    private void Awake()
    {
        Singleton = this;
    }
    
    void Init()
    {
        state = new TOFState();
    }

    public void StartGame()
    {
        Init();
        
        //creating empty board
        state.dynamicPieceBoard = new DynamicPiece[TOFGameMode.Singleton.boardSize.x, TOFGameMode.Singleton.boardSize.y];

        for (int x = 0; x < TOFGameMode.Singleton.boardSize.x; x++)
        {
            for (int y = 0; y < TOFGameMode.Singleton.boardSize.y; y++)
            {
                state.dynamicPieceBoard[x, y] = DynamicPiece.Empty;
            }
        }
        
        state.staticPieceBoard = new StaticPiece[TOFGameMode.Singleton.boardSize.x, TOFGameMode.Singleton.boardSize.y];

        for (int x = 0; x < TOFGameMode.Singleton.boardSize.x; x++)
        {
            for (int y = 0; y < TOFGameMode.Singleton.boardSize.y; y++)
            {
                state.staticPieceBoard[x, y] = StaticPiece.Empty;
            }
        }

        //temporary wip TODO: Temp
        state.staticPieceBoard[5, 5] = StaticPiece.Wave;
        state.staticPieceBoard[5, 6] = StaticPiece.Wave;
        state.staticPieceBoard[9, 7] = StaticPiece.Wave;
        Wave.Waves.Add(new Vector2Int(5,5), new Wave(Wave.WaveDirection.Forward));
        Wave.Waves.Add(new Vector2Int(5,6), new Wave(Wave.WaveDirection.Right));
        Wave.Waves.Add(new Vector2Int(9,7), new Wave(Wave.WaveDirection.Left));

        Vector2Int whirlpoolBotLeftPos = new Vector2Int(2, 5);
        state.staticPieceBoard[whirlpoolBotLeftPos.x, whirlpoolBotLeftPos.y] = StaticPiece.Whirlpool;
        state.staticPieceBoard[whirlpoolBotLeftPos.x, whirlpoolBotLeftPos.y + 1] = StaticPiece.Whirlpool;
        state.staticPieceBoard[whirlpoolBotLeftPos.x + 1, whirlpoolBotLeftPos.y + 1] = StaticPiece.Whirlpool;
        state.staticPieceBoard[whirlpoolBotLeftPos.x + 1, whirlpoolBotLeftPos.y] = StaticPiece.Whirlpool;

        Vector2Int[] whirlpoolPositions = new[]
        {
            new Vector2Int(whirlpoolBotLeftPos.x, whirlpoolBotLeftPos.y),
            new Vector2Int(whirlpoolBotLeftPos.x, whirlpoolBotLeftPos.y + 1),
            new Vector2Int(whirlpoolBotLeftPos.x + 1, whirlpoolBotLeftPos.y + 1),
            new Vector2Int(whirlpoolBotLeftPos.x + 1, whirlpoolBotLeftPos.y)
        };
        
        //Whirlpool.Whirlpools.Add(whirlpoolPositions, new Whirlpool());

        //settings players at position
        for (int i = 0; i < NetworkManager.Singleton.Server.ClientCount; i++)
        {
            Vector2Int playerPos = TOFGameMode.Singleton.playerStartPositions[i];
            state.dynamicPieceBoard[playerPos.x, playerPos.y] = DynamicPiece.Ship;
        }
    }

    public void SetShipPosAndRot(ushort client, int move)
    {
        int rotation = TOFPlayer.players[client].rotation;
        
        Vector2Int startPos = TOFPlayer.players[client].position;
        
        if (move == 4)
        {
            Debug.Log("Gets called on death");
            
            state.dynamicPieceBoard[startPos.x, startPos.y] = DynamicPiece.Empty;
            return;
        }
        
        Vector2Int endPos = GetPieceEndPosition(move, startPos, rotation);

        TOFPlayer.players[client].rotation = GetPieceEndRotation(move, rotation);

        //modify pos based on board obstacle here? TODO: Temp
        StaticPiece endPosStaticPiece = state.staticPieceBoard[endPos.x, endPos.y];

        if (endPosStaticPiece == StaticPiece.Wave)
        { 
            endPos += Wave.Waves[endPos].GetEffect();
        }

        if (endPosStaticPiece == StaticPiece.Whirlpool)
        {
            
        }

        state.dynamicPieceBoard[startPos.x, startPos.y] = DynamicPiece.Empty;
        state.dynamicPieceBoard[endPos.x, endPos.y] = DynamicPiece.Ship;

        TOFPlayer.players[client].position = endPos;
    }

    public Vector2Int GetPieceEndPosition(int move, Vector2Int currentPosition, int currentRotation)
    {
        switch(move)
        {
            case 0: // static position
                return currentPosition;
            case 1: // Forward
                switch(currentRotation)
                {
                    case 0: return new Vector2Int(currentPosition.x, currentPosition.y + 1);
                    case 90: case -270: return new Vector2Int(currentPosition.x + 1, currentPosition.y);
                    case 180: case -180: return new Vector2Int(currentPosition.x, currentPosition.y - 1);
                    case 270: case -90: return new Vector2Int(currentPosition.x - 1, currentPosition.y);
                } break; 
            case 2: // Right
                switch(currentRotation)
                {
                    case 0: return new Vector2Int(currentPosition.x + 1, currentPosition.y + 1);
                    case 90: case -270: return new Vector2Int(currentPosition.x + 1, currentPosition.y - 1);
                    case 180: case -180: return new Vector2Int(currentPosition.x - 1, currentPosition.y - 1);
                    case 270: case -90: return new Vector2Int(currentPosition.x - 1, currentPosition.y + 1);
                } break;
            case 3: // Left 
                switch(currentRotation)
                {
                    case 0: return new Vector2Int(currentPosition.x - 1, currentPosition.y + 1);
                    case 90: case -270: return new Vector2Int(currentPosition.x + 1,  currentPosition.y + 1);
                    case 180: case -180: return new Vector2Int(currentPosition.x + 1, currentPosition.y - 1);
                    case 270: case -90: return new Vector2Int(currentPosition.x - 1, currentPosition.y - 1);
                }break; 
        }
        
        Debug.LogError("GetPieceEndPosition did not find a case");
        return new Vector2Int();
    }
    
    public int GetPieceEndRotation(int move, int currentRotation)
    {
        switch(move)
        {
            case 0: currentRotation = currentRotation;
                break;
            case 1: currentRotation = currentRotation;
                break;
            case 2: currentRotation += 90;
                break;
            case 3: currentRotation -= 90;
                break;
        }

        if (currentRotation == 360 || currentRotation == -360)
        {
            currentRotation = 0;
        }

        return currentRotation;
    }
    
    public static bool IsPiecePosOutOfBounds(int x, int y)
    {
        if (x < 0 || x > TOFGameMode.Singleton.boardSize.x - 1 || y < 0 || y > TOFGameMode.Singleton.boardSize.y - 1)
            return true;
        return false;
    }

    public bool ProjectileHitCheck(ushort client, int ShipRotation, bool rightShot, bool leftShot, out Vector2Int hitPos) 
    {
        //todo: This part misses for double shot
        
        Vector2Int pos = TOFPlayer.players[client].position;

        if (ShipRotation == 0)
        {
            if (rightShot && !leftShot)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (IsPiecePosOutOfBounds(pos.x + i, pos.y))
                    {
                        hitPos = new Vector2Int(int.MaxValue, int.MaxValue);
                        return false;
                    }

                    if (state.dynamicPieceBoard[(int)pos.x + i, (int)pos.y] == DynamicPiece.Ship)
                    {
                        hitPos = new Vector2Int((int)pos.x + i, (int)pos.y);
                        return true;
                    }
                }
            }
            else if (leftShot && !rightShot)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (IsPiecePosOutOfBounds((int)pos.x - i, (int)pos.y))
                    {
                        hitPos = new Vector2Int(int.MaxValue, int.MaxValue);
                        return false;
                    }

                    if (state.dynamicPieceBoard[(int)pos.x - i, (int)pos.y] == DynamicPiece.Ship)
                    {
                        hitPos = new Vector2Int((int)pos.x - i, (int)pos.y);
                        return true;
                    }
                }
            }
        }
        else if (ShipRotation == 90)
        {
            if (rightShot && !leftShot)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (IsPiecePosOutOfBounds((int)pos.x, (int)pos.y - i))
                    {
                        hitPos = new Vector2Int(int.MaxValue, int.MaxValue);
                        return false;
                    }

                    if (state.dynamicPieceBoard[(int)pos.x, (int)pos.y - i] == DynamicPiece.Ship)
                    {
                        hitPos = new Vector2Int((int)pos.x, (int)pos.y - i);
                        return true;
                    }
                }
            }
            else if (leftShot && !rightShot)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (IsPiecePosOutOfBounds((int)pos.x, (int)pos.y + i))
                    {
                        hitPos = new Vector2Int(int.MaxValue, int.MaxValue);
                        return false;
                    }

                    if (state.dynamicPieceBoard[(int)pos.x, (int)pos.y + i] == DynamicPiece.Ship)
                    {
                        hitPos = new Vector2Int((int)pos.x, (int)pos.y + i);
                        return true;
                    }
                }
            }
        }
        if (ShipRotation == 180)
        {
            if (rightShot && !leftShot)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (IsPiecePosOutOfBounds((int)pos.x - i, (int)pos.y))
                    {
                        hitPos = new Vector2Int(int.MaxValue, int.MaxValue);
                        return false;
                    }

                    if (state.dynamicPieceBoard[(int)pos.x - i, (int)pos.y] == DynamicPiece.Ship)
                    {
                        hitPos = new Vector2Int((int)pos.x - i, (int)pos.y);
                        return true;
                    }
                }
            }
            else if (leftShot && !rightShot)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (IsPiecePosOutOfBounds((int)pos.x + i, (int)pos.y))
                    {
                        hitPos = new Vector2Int(int.MaxValue, int.MaxValue);
                        return false;
                    }

                    if (state.dynamicPieceBoard[(int)pos.x + i, (int)pos.y] == DynamicPiece.Ship)
                    {
                        hitPos = new Vector2Int((int)pos.x + i, (int)pos.y);
                        return true;
                    }
                }
            }
        }
        else if (ShipRotation == 270)
        {
            if (rightShot && !leftShot)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (IsPiecePosOutOfBounds((int)pos.x, (int)pos.y + i))
                    {
                        hitPos = new Vector2Int(int.MaxValue, int.MaxValue);
                        return false;
                    }

                    if (state.dynamicPieceBoard[(int)pos.x, (int)pos.y + i] == DynamicPiece.Ship)
                    {
                        hitPos = new Vector2Int((int)pos.x, (int)pos.y + i);
                        return true;
                    }
                }
            }
            else if (leftShot && !rightShot)
            {
                for (int i = 1; i <= 3; i++)
                {
                    if (IsPiecePosOutOfBounds((int)pos.x, (int)pos.y - i))
                    {
                        hitPos = new Vector2Int(int.MaxValue, int.MaxValue);
                        return false;
                    }

                    if (state.dynamicPieceBoard[(int)pos.x, (int)pos.y - i] == DynamicPiece.Ship)
                    {
                        hitPos = new Vector2Int((int)pos.x, (int)pos.y - i);
                        return true;
                    }
                }
            }
        }
        
        hitPos = new Vector2Int(int.MaxValue, int.MaxValue);
        return false;
    }

    public TOFPlayer DealDamageToPlayer(Vector2Int hitPosition)
    {
        foreach(TOFPlayer player in TOFPlayer.players.Values) 
        {    
            if(player.position == hitPosition)
            {
                player.TakeDamage(1);

                string text = "Player " + player.id + " has " + player.CurrentHealth +  " remaining";
                
                Debug.Log(text);
                TOFChatManager.SendServerChatMessage(text);
                
                return player;
            }
        }

        Debug.LogError("Warning: Deal damage failed. You are not supposed to ever reach here");
        return null;
    }
}
