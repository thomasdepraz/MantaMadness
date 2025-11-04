using System.Collections.Generic;
using UnityEngine;

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
            if(data.checkpoints[key] == true)
            {
                currentCheckpoint = key;
                print(key);
            }
        }

        foreach (WorldCheckpoint check in checkpoints)
        {
            if(check.indexName == currentCheckpoint)
            {

                SetStartCheckpoint(check.respawnTransform);

                Vector3 pos = Vector3.zero;
                Quaternion rotation;
                Game.Instance.Respawn(out pos, out rotation);
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        //Nothing to save
    }

    public void SetStartCheckpoint(Transform respawnTransform)
    {
        Game.Instance.SetRespawnTransform(respawnTransform);
    }

    public void SetCheckpoint(Transform respawnTransform, string checkpointIndexName, bool canDisplay, string displayName)
    {
        if(checkpointIndexName != currentCheckpoint)
        {
            currentCheckpoint = checkpointIndexName;

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
