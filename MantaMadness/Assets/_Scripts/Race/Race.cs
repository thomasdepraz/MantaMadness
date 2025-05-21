using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class Race : MonoBehaviour, ITimer, ICoinObjective
{
    [Header("Race parameters")]
    [SerializeField] private int lapCount;
    [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

    public int CheckpointCount { get => checkpoints.Count;}
    public int CurrentLap => currentLapCount;
    public int MaxLaps => lapCount;

    public Coin unlockedCoin;
    public Coin coinToUnlock => unlockedCoin;

    private int currentLapCount;
    private int checkpointCountThisLap;
    private Checkpoint startCheckpoint;
    private Checkpoint lastCheckpointPassed;
    public float currentTimer;

    public void Initialize()
    {
        startCheckpoint = checkpoints[0];
        lastCheckpointPassed = startCheckpoint;

        for (int i = 0; i < checkpoints.Count; i++)
        {
            checkpoints[i].Activate(i);
            checkpoints[i].checkpointPassed += CheckpointPassed;
        }

        checkpoints[1].Reset();
        checkpoints[1].ToggleOutline(true);
        currentLapCount = 1;
        checkpointCountThisLap = 0;
        currentTimer = 0;
        enabled = true;
    }

    private void CheckpointPassed(Checkpoint checkpoint)
    {
        lastCheckpointPassed = checkpoint;

        if(checkpoint == startCheckpoint)
        {
            checkpointCountThisLap = 0;
            if(++currentLapCount > lapCount)
            {
                EndRace();
                return;
            }
        }
        else
        {
            checkpointCountThisLap++;
        }

        checkpoint.ToggleOutline(false);
        int nextIndex = checkpoint.RaceIndex + 1 >= CheckpointCount ? 0 : checkpoint.RaceIndex + 1;
        checkpoints[nextIndex].Reset();
        checkpoints[nextIndex].ToggleOutline(true);

        Game.Instance.player.UpdateRaceTarget(checkpoints[nextIndex].transform);
    }

    private void EndRace()
    {
        for (int i = 0; i < CheckpointCount; i++)
        {
            checkpoints[i].checkpointPassed -= CheckpointPassed;
            checkpoints[i].Deactivate();
        }

        enabled = false;
        UnlockCoin();
        Game.Instance.raceManager.EndRace();
    }

    private void Update()
    {
        currentTimer += Time.deltaTime;
    }

    public Transform GetRespawnTransform()
    {
        return lastCheckpointPassed.respawnTransform;
    }

    public Transform GetFirstCheckpointTransform()
    {
        return checkpoints[1].transform;
    }

    public Transform GetStartTransform()
    {
        return startCheckpoint.respawnTransform;
    }

    float ITimer.GetTime()
    {
        return currentTimer;
    }

    public void UnlockCoin()
    {
        unlockedCoin?.gameObject.SetActive(true);
        //do camera event ?
    }
}
