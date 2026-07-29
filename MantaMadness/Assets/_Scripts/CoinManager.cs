using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class CoinManager : MonoBehaviour, IDataPersistence
{
    public static CoinManager Instance;
    public CoinHolder[] coinHolders;
    public bool AreAllCoinsCollected()
    {
        return PickupCoinCount >= coinHolders.Length;
    }

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
    private int pickupCointCount;
    public int PickupCoinCount
    { 
        get => pickupCointCount;
        set 
        {
            pickupCointCount = value;
            coinPickedUp?.Invoke(value);
        } 
    }

    public Action<int> clamPickedUp;
    private int clamCollectibleCount;
    public int ClamCollectibleCount
    {
        get => clamCollectibleCount;
        set
        {
            clamCollectibleCount = value;
            clamPickedUp?.Invoke(value);
        }
    }

    public Action<int> buckiePickedUp;
    private int buckieCollectibleCount;
    public int BuckieCollectibleCount
    {
        get => buckieCollectibleCount;
        set
        {
            buckieCollectibleCount = value;
            buckiePickedUp?.Invoke(value);
        }
    }

    public void LoadData(GameData data)
    {
        clamCollectibleCount = data.clamCount;
        buckieCollectibleCount = data.buckieCount;

        foreach(KeyValuePair<string, bool> pair in data.coinsCollected)
        {
            if (pair.Value)
            {
                pickupCointCount++;
            }
        }

        StartCoroutine(LateLoadUpdate(data));
    }

    public IEnumerator LateLoadUpdate(GameData data)
    {
        yield return new WaitForSeconds(0.1f);
        UIManager.Instance.gameInterface.UpdateClamCount(data.clamCount);
        UIManager.Instance.gameInterface.UpdateCoinCount(pickupCointCount);
        UIManager.Instance.gameInterface.UpdateBuckieCount(data.buckieCount);
    }

    public void SaveData(ref GameData data)
    {
        data.clamCount = clamCollectibleCount;
        data.buckieCount = buckieCollectibleCount;
    }

    public void PickupCoin()
    {
        PickupCoinCount++;

        //Check if end game possible
        Game.Instance.ActivateEndingScreen();
    }

    public void ResetCoinCount()
    {
        PickupCoinCount = 0;
    }

    public void PickupClam(int addValue)
    {
        ClamCollectibleCount += addValue;
    }

    public void PickupBuckie(int addValue)
    {
        BuckieCollectibleCount += addValue;
    }

    public void ActivateCoinHolder(string coinName)
    {
        for(int i = 0; i < coinHolders.Length; i++)
        {
            if (coinHolders[i].coinName == coinName)
            {
                coinHolders[i].startProcess();
            }
        }
    }

    public void ForceActivateCoinHolder(string coinName)
    {
        for (int i = 0; i < coinHolders.Length; i++)
        {
            if (coinHolders[i].coinName == coinName)
            {
                coinHolders[i].ForceSpawn();
            }
        }

    }
}
