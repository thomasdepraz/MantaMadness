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
        currentLapCount = 0;
        checkpointCountThisLap = 0;
    }

    private void CheckpointPassed(Checkpoint checkpoint)
    {
        lastCheckpointPassed = checkpoint;

        if(checkpoint == startCheckpoint)
        {
            checkpointCountThisLap = 0;
            if(++currentLapCount >= lapCount)
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

    }
}
