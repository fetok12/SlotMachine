using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Random = System.Random;

public struct CoinType
{
    public const float Jackpot = -1.25f;
    public const float A = 1.25f;
    public const float Bonus = 3.75f;
    public const float Seven = 6.25f;
    public const float Wild = 8.75f;
}

public enum SlotStatus
{
    Inactive,
    Started,
    Spinning,
    Stopping,
    Stopped
}

public class SpinResult
{
    public List<float> CoinPositions;
    public int SpinProbability;
    public int BlockWidth;
    public float BingoDensity;
    public List<int> BlockIntervals = new List<int>();


    public SpinResult(List<float> m_CoinPositions, int m_SpinProbability, int m_BlockWidth, float m_BingoDensity = 0)
    {
        CoinPositions = m_CoinPositions;

        // SpinProbability also equals to number of blocks
        SpinProbability = m_SpinProbability;
        BlockWidth = m_BlockWidth;
        BingoDensity = m_BingoDensity;

        for (int i = 0; i < m_SpinProbability; i++)
        {
            BlockIntervals.Add(m_BlockWidth);
        }

        //Check if total number of blocks exceeds 100
        var NumberToExtract = 0;
        if (m_SpinProbability * m_BlockWidth > 100) {
            NumberToExtract = m_SpinProbability * m_BlockWidth - 100; 
        }

        for (int i = 0; i < NumberToExtract; i++)
        {
            BlockIntervals[i] = BlockIntervals[i] - 1;
        }

        BlockIntervals.Shuffle();
    }
}

public class GameManager : MonoBehaviour
{
   

}