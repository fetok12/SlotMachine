using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    public static void SaveData(Dictionary<int, SpinResult> rounds, int currentRound)
    {

        PlayerPrefs.SetInt("RoundCount", rounds.Count);

        int index = 0;
        foreach (var round in rounds.Keys)
        {
            PlayerPrefs.SetString("Round" + index, $"{round},{rounds[round].CoinPositions[0]},{rounds[round].CoinPositions[1]},{rounds[round].CoinPositions[2]}");
            index++;
        }

        PlayerPrefs.SetInt("CurrentRound", currentRound);

    }

    public static int LoadData(ref Dictionary<int, SpinResult> rounds, ref int currentRound, List<SpinResult> sArray)
    {
        int RoundCount = PlayerPrefs.GetInt("RoundCount");
        for (int i = 0; i <RoundCount; i++)
        {
            string[] tokens = PlayerPrefs.GetString("Round" + i).Split(',');

            int sIndex = sArray.FindIndex(x => String.Join(",", x.CoinPositions) == $"{tokens[1]},{tokens[2]},{tokens[3]}");
            if(sIndex != -1)
            {
                rounds.Add(int.Parse(tokens[0]), sArray[sIndex]);
            }

        }
        currentRound = PlayerPrefs.GetInt("CurrentRound");

        if (rounds.Count == 0) return -1;

        return 1;

    }
}
