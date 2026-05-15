using UnityEngine;
using System.Collections.Generic;

public class CollectibleAreaRegistry : MonoBehaviour, IDataPersistence
{
    public static CollectibleAreaRegistry Instance;

    [SerializeField]
    public List<CollectibleAreaManager> areas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetCurrentArea(string areaID)
    {
        for (int i = 0; i < areas.Count; i++)
        {
            if (areas[i].AreaID == areaID)
            {
                CollectibleAreaManager.CurrentArea = areas[i];

                if (UIManager.Instance != null &&
                   UIManager.Instance.gameInterface != null)
                {
                    UIManager.Instance.gameInterface.RefreshAllAreaCount();
                    UIManager.Instance.gameInterface.UpdateAreaName(areas[i].areaName);
                }

                Debug.Log("Current Collectible Area = " + areaID);

                return;
            }
        }

        Debug.LogWarning("Collectible Area not found : " + areaID);
    }

    public void LoadData(GameData data)
    {
        //Debug.LogWarning("SET COLLECTIBLE AREA : " + data.currentCollectibleAreaID);
        //SetCurrentArea(data.currentCollectibleAreaID);
    }

    public void SaveData(ref GameData data)
    {
        //data.currentCollectibleAreaID = CollectibleAreaManager.CurrentArea.AreaID;
    }
}
