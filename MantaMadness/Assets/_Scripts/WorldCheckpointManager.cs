using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Collections;

public class WorldCheckpointManager : MonoBehaviour, IDataPersistence
{
    public static WorldCheckpointManager Instance;
    public string currentCheckpoint {  get; private set; }
    public List<WorldCheckpoint> checkpoints;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
    }

    public void LoadData(GameData data)
    {
        var keys = new List<string>(data.checkpoints.Keys);

        foreach (var key in keys)
        {
            if (data.checkpoints[key] == true)
            {
                currentCheckpoint = key;
                print(key);
            }
        }

        StartCoroutine(DelayLoadData());
        //foreach (WorldCheckpoint check in checkpoints)
        //{
        //    if(check.indexName == currentCheckpoint)
        //    {
        //        print("checkpoint loading");
        //        SetStartCheckpoint(check.respawnTransform);
        //        Vector3 pos = Vector3.zero;
        //        Quaternion rotation;
        //        Game.Instance.Respawn(out pos, out rotation);
        //    }
        //}
    }

    private IEnumerator DelayLoadData()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (WorldCheckpoint check in checkpoints)
        {
            if (check.indexName == currentCheckpoint)
            {
                print("checkpoint loading");
                SetStartCheckpoint(check.respawnTransform);
                Vector3 pos = Vector3.zero;
                Quaternion rotation;
                Game.Instance.Respawn(out pos, out rotation);
                check.EnableMat();
            }
            else
            {
                check.DisableMat();
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        //Nothing to save
        foreach(WorldCheckpoint worldPoint in checkpoints)
        {
            if (data.checkpoints.ContainsKey(worldPoint.indexName))
            {
                data.checkpoints.Remove(worldPoint.indexName);
            }

            if(worldPoint.indexName == currentCheckpoint)
            {
                data.checkpoints.Add(worldPoint.indexName, true);
            }
            else
            {
                data.checkpoints.Add(worldPoint.indexName, false);
            }
        }
    }

    public void SetStartCheckpoint(Transform respawnTransform)
    {
        Game.Instance.SetRespawnTransform(respawnTransform);
    }

    public void SetCheckpoint(Transform respawnTransform, string checkpointIndexName, bool canDisplay, string displayName)
    {
        if(checkpointIndexName != currentCheckpoint)
        {
            //Reset visual of all checkpoint
            foreach (WorldCheckpoint worldPoint in checkpoints)
            {
                if(worldPoint.indexName == currentCheckpoint)
                {
                    worldPoint.DisableMat();
                }
            }

            //Set New checkpoint
            currentCheckpoint = checkpointIndexName;

            foreach (WorldCheckpoint worldPoint in checkpoints)
            {
                if (worldPoint.indexName == currentCheckpoint)
                {
                    worldPoint.EnableMat();
                }
            }

            GameData data = DataPersistenceManager.Instance.gameData;

            //Manage Save Data Checkpoints
            var keys = new List<string>(data.checkpoints.Keys);

            foreach (var key in keys)
            {
                data.checkpoints[key] = false;
            }

            if (data.checkpoints.ContainsKey(currentCheckpoint))
            {
                data.checkpoints.Remove(currentCheckpoint);
            }

            data.checkpoints.Add(currentCheckpoint, true);

            Game.Instance.SetRespawnTransform(respawnTransform);
            if(canDisplay)
            {
                UIManager.Instance.gameInterface.StartDisplayCoroutine(displayName);
            }
        }
    }
}
