using System;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;
    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private CoinManager() { }

    public Action<int> coinPickedUp;
    public int PickupCoinCount 
    { 
        get => PlayerPrefs.GetInt(Constants.c_CoinAmountSave, 0);
        set 
        {
            PlayerPrefs.SetInt(Constants.c_CoinAmountSave, value);
            coinPickedUp?.Invoke(value);
        } 
    }

#if UNITY_EDITOR
    public void Start()
    {
        PickupCoinCount = 0;
    }
#endif

    public void PickupCoin()
    {
        PickupCoinCount++;
    }
}
