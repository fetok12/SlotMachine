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
    public List<SpinResult> SpinResults = new List<SpinResult>
    {
        new SpinResult(new List<float>{CoinType.A, CoinType.Wild, CoinType.Bonus }, 13, Mathf.CeilToInt(100f/13f)),
        new SpinResult(new List<float>{CoinType.Wild, CoinType.Wild, CoinType.Seven }, 13, Mathf.CeilToInt(100f/13f)),
        new SpinResult(new List<float>{CoinType.Jackpot, CoinType.Jackpot, CoinType.A }, 13, Mathf.CeilToInt(100f/13f)),
        new SpinResult(new List<float>{CoinType.Wild, CoinType.Bonus, CoinType.A }, 13, Mathf.CeilToInt(100f/13f)),
        new SpinResult(new List<float>{CoinType.Bonus, CoinType.A, CoinType.Jackpot }, 13, Mathf.CeilToInt(100f/13f)),
        new SpinResult(new List<float>{CoinType.A, CoinType.A, CoinType.A }, 9, Mathf.CeilToInt(100f/9f), 5f),
        new SpinResult(new List<float>{CoinType.Bonus, CoinType.Bonus, CoinType.Bonus }, 8, Mathf.CeilToInt(100f/8f), 10f),
        new SpinResult(new List<float>{CoinType.Seven, CoinType.Seven, CoinType.Seven }, 7, Mathf.CeilToInt(100f/7f), 20f),
        new SpinResult(new List<float>{CoinType.Wild, CoinType.Wild, CoinType.Wild }, 6, Mathf.CeilToInt(100f/6f), 28f),
        new SpinResult(new List<float>{CoinType.Jackpot, CoinType.Jackpot, CoinType.Jackpot }, 5, Mathf.CeilToInt(100f/5f), 40f),
    };


    //Round Number and Coins
    public Dictionary<int, SpinResult> Rounds = new Dictionary<int, SpinResult>();

    private int currentRound = 0;

    // SlotId and Status
    public Dictionary<int, SlotStatus> Slots = new Dictionary<int, SlotStatus>();

    public List<GameObject> Rows = new List<GameObject>();
    public static event Action<SpinResult> SpinEvent;

    public static event Action<List<float>> LoadData;

    public GameObject RoundIndicatorUI;
    public GameObject BingoParticle;

    private bool ShowBingoAnim = false;
    private float BingoMagnitude;
    public void CreateRounds()
    {
        Rounds.Clear();
        // Randomize Order
        //SpinResults.Shuffle();

        List<int> CreatedRounds = new List<int>();

        for (int i = 0; i < SpinResults.Count; i++)
        {
            int LeftValue = 0;

            // Pick random round number for every block 
            for (int j = 0; j < SpinResults[i].SpinProbability; j++)
            {
                int RightValue = LeftValue + SpinResults[i].BlockIntervals[j] - 1;
                
                int RandomRound = GenerateRoundNumber(ref CreatedRounds, LeftValue, RightValue);
                
                // if RandomRound equals to -1, Rerun the CreateRounds function.
                if(RandomRound == -1)
                {
                    foreach (var item in SpinResults)
                    {
                        item.BlockIntervals.Shuffle();
                    }
                    CreateRounds();
                    return;
                }
                Rounds.Add(RandomRound, SpinResults[i]);
                LeftValue = RightValue + 1;
            }

        }
        PlayerPersistence.SaveData(Rounds, currentRound);
    }


    public int GenerateRoundNumber(ref List<int> CreatedRounds, int LeftValue, int RightValue)
    {
        List<int> AllNumbers = new List<int>();
        for (int i = LeftValue; i < RightValue + 1; i++)
        {
            AllNumbers.Add(i);
        }

        List<int> CandidateNumbers = AllNumbers.Except(CreatedRounds).ToList();

        if(CandidateNumbers.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, CandidateNumbers.Count);
            CreatedRounds.Add(CandidateNumbers[index]);
            return CandidateNumbers[index];
        }

        return -1;
    }


    private void Awake()
    {
        Row.SlotStatusEvent += UpdateSlotStatus;
    }
    private void Start()
    {
        if (PlayerPersistence.LoadData(ref Rounds, ref currentRound, SpinResults) == -1)
            CreateRounds();
        else
        {
            LoadPreviousState();

        }
    
        ShowLogs();

    }


    public void PlayRound()
    {
        if (currentRound == Rounds.Count)
            ResetGameState();

        // Check if slots are ready
        if (Slots.Values.Any(x => x != SlotStatus.Inactive)) return;
      

        // Check if all coins in the list are the same.
        bool isBingo = !Rounds[currentRound].CoinPositions.Distinct().Skip(1).Any();

        ShowBingoAnim = isBingo;
        BingoMagnitude = Rounds[currentRound].BingoDensity;

        // Send Rows SpinResult
        SpinEvent?.Invoke(Rounds[currentRound]);

        RoundIndicatorUI.GetComponent<TextMeshProUGUI>().text = $"Round: {currentRound + 1}";

        currentRound++;
      
        PlayerPersistence.SaveData(Rounds, currentRound);

        Debug.Log(currentRound);
    }

    public void UpdateSlotStatus(int SlotNumber, SlotStatus Status)
    {
        if (!Slots.ContainsKey(SlotNumber))
        {
            Slots.Add(SlotNumber, Status);
            return;
        }
        Slots[SlotNumber] = Status;

        // Check if all slots stopped
        bool isStop = !Slots.Values.Any(x => x != SlotStatus.Inactive);
      
        if (isStop)
        {
            // If it's bingo
            if (ShowBingoAnim)
            {
                BingoParticle.SetActive(true);
                var ps = BingoParticle.GetComponent<ParticleSystem>().main;
                ps.simulationSpeed = 2.33f;

                //Apply BingoMagnitude
                var emission = BingoParticle.GetComponent<ParticleSystem>().emission;
                Debug.Log(BingoMagnitude);
                emission.rateOverTime = BingoMagnitude;

                BingoParticle.GetComponent<ParticleSystem>().Play();
            }
        }

    }


}


