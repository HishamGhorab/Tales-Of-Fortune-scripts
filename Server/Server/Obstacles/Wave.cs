using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wave
{
    public Wave(WaveDirection direction)
    {
        waveDirection = direction;
    }
    
    public static Dictionary<Vector2Int, Wave> Waves = new Dictionary<Vector2Int, Wave>();

    public enum WaveDirection
    {
        Left,
        Right,
        Forward,
        Backward
    }

    public WaveDirection waveDirection;

    public Vector2Int GetEffect()
    {
        switch (waveDirection)
        {
            case WaveDirection.Left:
                return Vector2Int.left;
                break;
            case WaveDirection.Right:
                return Vector2Int.right;
                break;
            case WaveDirection.Forward:
                return Vector2Int.up;
                break;
            case WaveDirection.Backward:
                return Vector2Int.down;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
