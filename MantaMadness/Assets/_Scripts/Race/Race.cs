using System.Collections.Generic;
using UnityEngine;

public class RaceUtility
{
    public static void StartRace(Race race)
    {
        if(race.CheckpointCount < 2)
        {
            Debug.LogError("A race should have more than two checkpoints");
            return;
        }

        race.Initialize();
    }
}

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

        UIManager.Instance.raceInterface.Init(this);
    }

    private void CheckpointPassed(Checkpoint checkpoint)
    {
        lastCheckpointPassed = checkpoint;
        Debug.Log($"Checkpoint {checkpoint.RaceIndex} passed");

        if(checkpoint == startCheckpoint)
        {
            Debug.Log($"LapCount = {currentLapCount + 1} / {lapCount}");
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
        //Victory animation
        Debug.Log($"RaceEnded");
        for (int i = 0; i < CheckpointCount; i++)
        {
            checkpoints[i].checkpointPassed -= CheckpointPassed;
            checkpoints[i].Deactivate();
        }

        UIManager.Instance.raceInterface.Hide();
    }

    public Transform GetRespawnTransform()
    {
        return lastCheckpointPassed.respawnTransform;
    }
}
