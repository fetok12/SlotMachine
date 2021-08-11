using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class Row : MonoBehaviour
{
    private bool StartSpinning = false;
    private float NextTimeCanSpin;
    private int m_RowIndex;
    private bool StartStopping = false;
    private float SlowDownTime;
    private float DelayStart;
    private float m_Coin;
    private bool FastSlowMode = false;

    public List<GameObject> CoinObjects;

    public static event Action<int, SlotStatus> SlotStatusEvent;

    private void Start()
    {
        m_RowIndex = transform.GetSiblingIndex();
        SlotStatusEvent?.Invoke(m_RowIndex, SlotStatus.Inactive);
        GameManager.SpinEvent += StartSpinEvent;
        GameManager.LoadData += LoadPreviousState;

    }

    public void StartSpinEvent(SpinResult spinResult)
    {
        SlotStatusEvent?.Invoke(m_RowIndex, SlotStatus.Started);
        m_Coin = spinResult.CoinPositions[m_RowIndex];
        NextTimeCanSpin = Time.time + (m_RowIndex == 0 ? 3f : 3.2f);
        DelayStart = m_RowIndex == 1 ? Time.time + 0.15f : Time.time + 0.3f;
        StartSpinning = true;
    }

    public void ToggleCoinBlur(bool isOn)
    {
        foreach (var item in CoinObjects)
        {
            item.transform.GetChild(0).gameObject.SetActive(!isOn);
            item.transform.GetChild(1).gameObject.SetActive(isOn);
        }
    }

    public void LoadPreviousState(List<float> coins)
    {
        transform.localPosition = new Vector2(transform.localPosition.x, coins[m_RowIndex]);
    }

    private void StopRow(float SlowDownTime)
    {
        //Debug.Log("Stopping...");
        if (transform.localPosition.y <= -1.25f)
        {
            transform.localPosition = new Vector2(transform.localPosition.x, 11.25f);
        }

        if (Time.time >= SlowDownTime && Mathf.Abs(transform.localPosition.y - m_Coin) < 0.1f)
        {
            transform.localPosition = Vector2.Lerp(transform.localPosition, new Vector2(transform.localPosition.x, m_Coin), Time.deltaTime * 10f);

            if (transform.localPosition.y - m_Coin < 0.01f)
            {
                StartSpinning = false;
                StartStopping = false;
                FastSlowMode = false;
                SlotStatusEvent?.Invoke(m_RowIndex, SlotStatus.Inactive);
              
            }
            return;
        }
        float SlowingRate = FastSlowMode == true ? 2.5f : 4.25f; 
        //if(m_RowIndex == 2)
        //{
        //    Debug.Log("SLOWRATE");
        //    Debug.Log(SlowingRate);
        //}
        float LerpSpeed = m_RowIndex == 2 ? Time.deltaTime * ( 10f - (Time.time - NextTimeCanSpin) * SlowingRate) : Time.deltaTime * 10f;
        if (LerpSpeed <= 0.001f) { LerpSpeed =  0.001f; }

        transform.localPosition = Vector2.Lerp(transform.localPosition, new Vector2(transform.localPosition.x, transform.localPosition.y - 2.50f), LerpSpeed);
    }


    private void Update()
    {
        if (!StartSpinning) return;

        // Add small delay except the first row
        if (Time.time < DelayStart && m_RowIndex != 0) return;

        if (Time.time > NextTimeCanSpin)
        {
            // STOPPING... & SLOWING DOWN...
            if (!StartStopping)
            {
                int SlowMode = Random.Range(0, 2);
                FastSlowMode = SlowMode != 1;
                StartStopping = true;
                SlotStatusEvent?.Invoke(m_RowIndex, SlotStatus.Stopping);
                ToggleCoinBlur(false);
                SlowDownTime = m_RowIndex == 2 ? SlowMode == 1 ? 1f : 2.25f : 0f;
                SlowDownTime += Time.time;
            }
            StopRow(SlowDownTime);
            return;

        }

        ToggleCoinBlur(true);
        //SPINNING...
        SlotStatusEvent?.Invoke(m_RowIndex, SlotStatus.Spinning);

        if (transform.localPosition.y <= -1.25f)
            transform.localPosition = new Vector2(transform.localPosition.x, 11.25f);


        transform.localPosition = Vector2.Lerp(transform.localPosition, new Vector2(transform.localPosition.x, transform.localPosition.y - 2.50f), Time.deltaTime * 10f);

    }

}
