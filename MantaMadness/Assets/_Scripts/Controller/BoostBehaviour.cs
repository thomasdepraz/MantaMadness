using System;
using System.Collections.Generic;
using UnityEngine;

public enum BoostAction
{
    Trick,
    PerfectJump, 
    BoostedDrift, 
    RailQTE, 
    Buoy, 
    CoolSun, 
    Collectible
}

[System.Serializable]
public struct BoostActionValue
{
    public BoostAction action;
    public int value;
}

public class BoostBehaviour : MonoBehaviour
{
    public Action<bool> useBoost;
    public Action<int> incrementGauge;

    [Header("Parameters")]
    public int maxBoost = 100;
    public int boostStep = 25;
    public List<BoostActionValue> actionValues = new List<BoostActionValue>();

    private int boostCount;
    private Dictionary<BoostAction, int> actionValueTable = new Dictionary<BoostAction, int>();

    void Start()
    {
        boostCount = 0;
        foreach (BoostActionValue actionValue in actionValues)
        {
            actionValueTable[actionValue.action] = actionValue.value;
        }
        actionValues.Clear();
    }

    public void IncrementGauge(BoostAction boostAction)
    {
        if(boostCount == maxBoost)
        {
            return;
        }

        int value = actionValueTable[boostAction];

        //value = Mathf.Min(value, maxBoost - boostCount);

        boostCount += value;
        boostCount = Math.Clamp(boostCount, 0, maxBoost);

        UIManager.Instance.boostGaugeInterface?.SetGauge(boostCount, maxBoost);
    }

    public bool UseBoost(Action toPerform)
    {
        if (boostCount < boostStep)
        {
            useBoost?.Invoke(false);
            return false;
        }

        boostCount -= boostStep;
        useBoost?.Invoke(true);
        UIManager.Instance.boostGaugeInterface?.SetGauge(boostCount, maxBoost);

        toPerform?.Invoke();
        return true;
    }
}
