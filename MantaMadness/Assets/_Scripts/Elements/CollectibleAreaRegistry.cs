using UnityEngine;
using System.Collections.Generic;

public class CollectibleAreaRegistry : MonoBehaviour
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
                    UIManager.Instance.gameInterface.RefreshAreaClamCount();
                    UIManager.Instance.gameInterface.RefreshAreaBuckieCount();
                    UIManager.Instance.gameInterface.UpdateAreaName(areas[i].areaName);
                }

                Debug.Log("Current Collectible Area = " + areaID);

                return;
            }
        }

        Debug.LogWarning("Collectible Area not found : " + areaID);
    }
}
