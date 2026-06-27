using System.Collections;
using UnityEngine;

public class CollectibleAreaManager : MonoBehaviour, IDataPersistence
{
    public static CollectibleAreaManager CurrentArea;

    //[SerializeField] private string areaID;
    [SerializeField] private CollectibleArea areaID;
    [SerializeField] public string areaName;

    private Collectible[] collectibles;
    [SerializeField]private CoinHolder[] suns;

    [SerializeField] private CoinHolder sunOnClearEnable;

    private bool allClamsRewardTriggered = false;

    public CollectibleArea AreaID => areaID;

    public int TotalCollectibles => collectibles.Length;

    private SteamSignal signal;

    public int CollectedCollectibles
    {
        get
        {
            int count = 0;

            foreach (var collectible in collectibles)
            {
                if (collectible.State == CollectibleState.Inactivable)
                {
                    count++;
                }
            }

            return count;
        }
    }

    private void Awake()
    {
        collectibles = GetComponentsInChildren<Collectible>(true);
        if(CollectibleAreaRegistry.Instance != null)
        {
            if (!CollectibleAreaRegistry.Instance.areas.Contains(this))
            {
                CollectibleAreaRegistry.Instance.areas.Add(this);
            }
        }

        if(GetComponent<SteamSignal>() != null)
        {
            signal  = GetComponent<SteamSignal>();
        }
    }

    public void EnterArea()
    {
        Debug.Log("Area " + AreaID + " Entered!");
        CurrentArea = this;

        if (UIManager.Instance != null && UIManager.Instance.gameInterface != null)
        {
            UIManager.Instance.gameInterface.RefreshAllAreaCount();
            UIManager.Instance.gameInterface.UpdateAreaName(areaName);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (CurrentArea == this)
        {
            data.currentCollectibleAreaID = areaID;
        }

        if (data.areaSunRewards.ContainsKey(areaID.ToString()))
        {
            
            data.areaSunRewards[areaID.ToString()] = allClamsRewardTriggered;
        }
        else
        {
            data.areaSunRewards.Add(areaID.ToString(),allClamsRewardTriggered);
        }
    }

    public void LoadData(GameData data)
    {
        StartCoroutine(LoadDataDelay(data));
    }

    private IEnumerator LoadDataDelay(GameData data)
    {
        yield return new WaitForSeconds(0.1f);
        if (data.areaSunRewards.TryGetValue(areaID.ToString(), out bool triggered))
        {
            allClamsRewardTriggered = triggered;
        }

        if (AreAllClamsCollected())
        {
            if (sunOnClearEnable != null)
                sunOnClearEnable.ForceSpawn();
        }

        if(CollectedCollectibles >= TotalCollectibles && CollectedSun >= TotalSun)
        {
            OnAllCollectiblesCollected();
        }

    }

    public int TotalClams
    {
        get
        {
            int total = 0;

            foreach (var collectible in collectibles)
            {
                if (collectible.IsBuckie())
                    continue;

                total += collectible.GetCollectibleValue();
            }

            return total;
        }
    }

    public int CollectedClams
    {
        get
        {
            int total = 0;

            foreach (var collectible in collectibles)
            {
                if (collectible.IsBuckie())
                    continue;

                if (collectible.State == CollectibleState.Inactivable)
                {
                    total += collectible.GetCollectibleValue();
                }
            }

            return total;
        }
    }

    public int TotalBuckies
    {
        get
        {
            int total = 0;

            foreach (var collectible in collectibles)
            {
                if (collectible.IsBuckie())
                {
                    total += collectible.GetCollectibleValue();
                }
            }
            return total;
        }
    }

    public int CollectedBuckies
    {
        get
        {
            int total = 0;

            foreach (var collectible in collectibles)
            {
                if (collectible.IsBuckie() &&
                   collectible.State == CollectibleState.Inactivable)
                {
                    total += collectible.GetCollectibleValue();
                }
            }

            return total;
        }
    }

    public int TotalSun
    {
        get
        {
            int total = 0;

            foreach (var sun in suns)
            {
                total += 1;
            }
            return total;
        }
    }

    public int CollectedSun
    {
        get
        {
            int total = 0;
            foreach (var sun in suns)
            {
                if (sun.hasBeenObtained)
                {
                    total += 1;
                }
            }
            return total;
        }
    }


    public string GetClamProgressText()
    {
        return CollectedClams + "/" + TotalClams;
    }

    public string GetBuckieProgressText()
    {
        return CollectedBuckies + "/" + TotalBuckies;
    }

    public string GetSunProgressText()
    {
        return CollectedSun + "/" + TotalSun;
    }

    public static void RestoreCurrentArea()
    {
        GameData data = DataPersistenceManager.Instance.gameData;

        foreach (var area in CollectibleAreaRegistry.Instance.areas)
        {
            {
                if (area.AreaID == data.currentCollectibleAreaID)
                {
                    CurrentArea = area;

                    if (UIManager.Instance != null && UIManager.Instance.gameInterface != null)
                    {
                        UIManager.Instance.gameInterface.RefreshAllAreaCount();
                        UIManager.Instance.gameInterface.UpdateAreaName(area.areaName);
                    }
                    return;
                }
            }
        }
    }
    public void CheckAllClamsCollected()
    {
        if (allClamsRewardTriggered)
            return;

        if (CollectedClams >= TotalClams)
        {
            allClamsRewardTriggered = true;

            Debug.Log("ALL CLAMS COLLECTED IN AREA : " + areaName);

            OnAllClamsCollected();
        }
    }

    public void CheckAllCollectibleCollected()
    {

        if(CollectedCollectibles >= TotalCollectibles && CollectedSun >= TotalSun)
        {
            OnAllCollectiblesCollected();
        }
    }

    public void OnAllClamsCollected()
    {
        if (sunOnClearEnable != null)
            CoinManager.Instance.ActivateCoinHolder(sunOnClearEnable.coinName);
    }

    public bool AreAllClamsCollected()
    {
        return CollectedClams >= TotalClams;
    }

    public void OnAllCollectiblesCollected()
    {
        if (signal != null)
        {
            signal.Trigger();
        }
    }
}
