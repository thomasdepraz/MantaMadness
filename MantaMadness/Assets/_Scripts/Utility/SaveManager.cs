using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private IEnumerable<ISaveable> saveables;
    private void Awake()
    {
        GetSaveables();

#if UNITY_EDITOR
        return;
#endif
        Load();

    }
    public void GetSaveables()
    {
        saveables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>();
    }

    private void OnApplicationQuit()
    {
        Save();    
    }

    public void Save()
    {
        foreach (var item in saveables)
        {
            item.Save();
        }
    }

    public void Load()
    {
        foreach(var item in saveables)
        {
            item.Load();
        }
    }

    public void DeleteAndLoad()
    {
        PlayerPrefs.DeleteAll();
        CoinManager.Instance.PickupCoinCount = 0;
        Load();
    }
}
