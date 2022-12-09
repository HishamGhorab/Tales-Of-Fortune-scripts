using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlpool
{
    public Whirlpool(WhirlPoolDirection direction)
    {
        whirlPoolDirection = direction;
    }
    
    public static Dictionary<Vector2Int[], Whirlpool> Whirlpools = new Dictionary<Vector2Int[], Whirlpool>();
    
    public enum WhirlPoolDirection
    {
        Left,
        Right,
        Forward,
        Backward
    }

    public WhirlPoolDirection whirlPoolDirection;
    
    public Vector2Int GetEffect()
    {
        switch (whirlPoolDirection)
        {
            case WhirlPoolDirection.Left:
                return Vector2Int.left;
                break;
            case WhirlPoolDirection.Right:
                return Vector2Int.right;
                break;
            case WhirlPoolDirection.Forward:
                return Vector2Int.up;
                break;
            case WhirlPoolDirection.Backward:
                return Vector2Int.down;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}