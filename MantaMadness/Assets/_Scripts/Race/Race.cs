using System.Collections.Generic;
using UnityEngine;

public class Race : MonoBehaviour
{
    [Header("Race parameters")]
    [SerializeField] private int lapCount;
    [SerializeField] private List<Checkpoint> checkpoints = new List<Checkpoint>();

    public int CheckpointCount { get => checkpoints.Count;}
    public int CurrentLap => currentLapCount;
    public int MaxLaps => lapCount;
    private int currentLapCount;
    private int checkpointCountThisLap;
    private Checkpoint startCheckpoint;
    private Checkpoint lastCheckpointPassed;

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
        currentLapCount = 1;
        checkpointCountThisLap = 0;
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
            }
        }
        else
        {
            checkpointCountThisLap++;
        }

        int nextIndex = checkpoint.RaceIndex + 1 >= CheckpointCount ? 0 : checkpoint.RaceIndex + 1;
        checkpoints[nextIndex].Reset();
    }

    private void EndRace()
    {
        for (int i = 0; i < CheckpointCount; i++)
        {
            checkpoints[i].checkpointPassed -= CheckpointPassed;
            checkpoints[i].Deactivate();
        }

        Game.Instance.raceManager.EndRace();
    }

    public Transform GetRespawnTransform()
    {
        return lastCheckpointPassed.respawnTransform;
    }

    public Transform GetStartTransform()
    {
        return startCheckpoint.respawnTransform;
    }
}
