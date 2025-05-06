using System;
using System.Collections;
using UnityEngine;

public class RaceManager
{
    public Race currentRace;

    public Action raceStarted;
    public Action raceEnded;


    public bool StartRace(Race race)
    {
        if(race is null)
        {
            Debug.LogError("Tried to start null race");
            return false;
        }

        if(race.CheckpointCount < 2)
        {
            Debug.LogError("A race should have more than two checkpoints");
            return false;
        }

        race.Initialize();
        currentRace = race;
        UIManager.Instance.raceInterface.Init(race);

        //set player position
        Game.Instance.player.ForcePosition(race.GetStartTransform());

        raceStarted.Invoke();
        return true;
    }

    public void EndRace()
    {
        UIManager.Instance.victoryScreen.Initialize(currentRace);
        ((IScreen)UIManager.Instance.victoryScreen).Show();
        currentRace = null;
        UIManager.Instance.raceInterface.Hide();
        raceEnded.Invoke();

        UIManager.Instance.StartCoroutine(HideVictory());
    }

    public bool TryGetRespawn(out Transform respawn)
    {
        respawn = null;
        if (currentRace is null)
            return false;

        respawn = currentRace.GetRespawnTransform(); ;
        return true;
    }

    private IEnumerator HideVictory()
    {
        yield return new WaitForSeconds(3f);

        ((IScreen)UIManager.Instance.victoryScreen).Hide();
    }
}
