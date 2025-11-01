using System;
using System.Collections;
using UnityEngine;

public class CoinManager : MonoBehaviour, IDataPersistence
{
    public static CoinManager Instance;

    public CoinHolder[] coinHolders;
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

    public Action<int> collectiblePickedUp;
    private int pickupCollectibleCount;
    public int PickupCollectibleCount
    {
        get => pickupCollectibleCount;
        set
        {
            pickupCollectibleCount = value;
            collectiblePickedUp?.Invoke(value);
        }
    }

#if UNITY_EDITOR
    public void Start()
    {
        PickupCoinCount = 0;
        //PickupCollectibleCount = 0;
    }
#endif

    public void LoadData(GameData data)
    {
        pickupCollectibleCount = data.clamCount;
        StartCoroutine(LateLoadUpdate(data));
    }

    public IEnumerator LateLoadUpdate(GameData data)
    {
        yield return new WaitForSeconds(0.1f);
        UIManager.Instance.gameInterface.UpdateCollectibleCount(data.clamCount);
    }

    public void SaveData(ref GameData data)
    {
        data.clamCount = pickupCollectibleCount;
    }

    public void PickupCoin()
    {
        PickupCoinCount++;
    }

    public void PickupCollectible(int addValue)
    {
        PickupCollectibleCount += addValue;
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
}
