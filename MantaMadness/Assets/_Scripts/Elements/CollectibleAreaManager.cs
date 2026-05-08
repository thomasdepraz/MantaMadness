using UnityEngine;

public class CollectibleAreaManager : MonoBehaviour, IDataPersistence
{
    public static CollectibleAreaManager CurrentArea;

    [SerializeField] private string areaID;
    [SerializeField] public string areaName;

    private Collectible[] collectibles;

    public string AreaID => areaID;

    public int TotalCollectibles => collectibles.Length;

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
    }

    public void EnterArea()
    {
        Debug.Log("Area " + AreaID + " Entered!");
        CurrentArea = this;

        if (UIManager.Instance != null && UIManager.Instance.gameInterface != null)
        {
            UIManager.Instance.gameInterface.RefreshAreaClamCount();
            UIManager.Instance.gameInterface.RefreshAreaBuckieCount();
            UIManager.Instance.gameInterface.UpdateAreaName(areaName);
        }
    }

    public void SaveData(ref GameData data)
    {
        if (CurrentArea == this)
        {
            data.currentCollectibleAreaID = areaID;
        }
    }

    public void LoadData(GameData data)
    {
        if (data.currentCollectibleAreaID == areaID)
        {
            CurrentArea = this;
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
    public string GetClamProgressText()
    {
        return CollectedClams + " / " + TotalClams;
    }

    public string GetBuckieProgressText()
    {
        Debug.Log("Buckie Area Count " +CollectedBuckies + " / " + TotalBuckies);
        return CollectedBuckies + " / " + TotalBuckies;
    }

    public static void RestoreCurrentArea()
    {
        CollectibleAreaManager[] areas =FindObjectsByType<CollectibleAreaManager>(FindObjectsInactive.Include,FindObjectsSortMode.None);

        GameData data = DataPersistenceManager.Instance.gameData;

        foreach (var area in areas)
        {
            if (area.AreaID == data.currentCollectibleAreaID)
            {
                CurrentArea = area;

                if (UIManager.Instance != null &&
                   UIManager.Instance.gameInterface != null)
                {
                    UIManager.Instance.gameInterface.RefreshAreaClamCount();
                    UIManager.Instance.gameInterface.RefreshAreaBuckieCount();
                    UIManager.Instance.gameInterface.UpdateAreaName(area.areaName);
                }
                return;
            }
        }
    }

}
