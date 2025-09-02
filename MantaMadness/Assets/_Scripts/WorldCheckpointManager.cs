using UnityEngine;

public class WorldCheckpointManager : MonoBehaviour
{
    public static WorldCheckpointManager Instance;
    public string currentCheckpoint {  get; private set; }

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
        }
    }

    public void SetCheckpoint(Transform respawnTransform, string checkpointIndexName, bool canDisplay, string displayName)
    {
        if(checkpointIndexName != currentCheckpoint)
        {
            currentCheckpoint = checkpointIndexName;
            Game.Instance.SetRespawnTransform(respawnTransform);
            if(canDisplay)
            {
                UIManager.Instance.gameInterface.StartDisplayCoroutine(displayName);
            }
        }
    }
}
