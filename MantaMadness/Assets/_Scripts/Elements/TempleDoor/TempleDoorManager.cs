using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TempleDoorManager : SpecialDestructibleManager, IDataPersistence
{
    [SerializeField] private List<TimelineAsset> statueCinematics = new();
    [SerializeField] private TimelineAsset doorOpeningCinematic;

    [SerializeField] private GameObject blueCrystal;
    [SerializeField] private GameObject redCrystal;
    [SerializeField] private GameObject yellowCrystal;
    [SerializeField] private GameObject greenCrystal;
    [SerializeField] private GameObject door;

    public void OnDestructibleDestroyed(TempleStatueDestructible destructible)
    {
        int index = destroyedCount - 1;

        if (index < statueCinematics.Count)
        {
            StartCoroutine(PlayStatueCinematic(index, destructible.type));
        }
    }

    protected IEnumerator PlayStatueCinematic(int index, TempleStatueType statueType)
    {

        Game.Instance.player.ForceLock(true);

        TimelineAsset cinematic = null;

        switch (statueType)
        {
            case TempleStatueType.Blue:
                cinematic = statueCinematics[0];
                break;
            case TempleStatueType.Yellow:
                cinematic = statueCinematics[1];
                break;
            case TempleStatueType.Green:
                cinematic = statueCinematics[2];
                break;
            case TempleStatueType.Red:
                cinematic = statueCinematics[3];
                break;
        }

        CinematicManager.instance.cinematicPlayer.stopped += OnTimelineStopped;
        CinematicManager.instance.PlayCinematic(cinematic);

        yield break;

    }

    private void OnTimelineStopped(PlayableDirector director)
    {
        CinematicManager.instance.cinematicPlayer.stopped -= OnTimelineStopped;

        if (destroyedCount >= destructibles.Count)
        {
            CinematicManager.instance.cinematicPlayer.stopped += OnDoorTimelineStopped;
            CinematicManager.instance.PlayCinematic(doorOpeningCinematic);
            return;
        }

        OnCinematicFinished();
    }

    private void OnDoorTimelineStopped(PlayableDirector director)
    {
        CinematicManager.instance.cinematicPlayer.stopped -= OnDoorTimelineStopped;
        OnCinematicFinished();
    }

    private void OnCinematicFinished()
    {
        if (destroyedCount >= destructibles.Count)
            DisableDoor();

        Game.Instance.player.ForceLock(false);

        CinematicManager.instance.ResetCam();
        MantaCameraController.instance.ActivatePlayerCamera();
    }

    public void LoadData(GameData data)
    {
        destroyedCount = 0;

        foreach (var destructible in destructibles)
        {
            if (destructible is TempleStatueDestructible statue)
            {
                if (data.puzzleElements.TryGetValue(statue.id, out bool state) && state)
                {
                    destroyedCount++;
                    ActivateCrystal(statue.type);
                }
            }
        }

        if (destroyedCount >= destructibles.Count)
        {
            DisableDoor();
        }
    }

    public void SaveData(ref GameData data)
    {
        //throw new System.NotImplementedException();
    }

    public void ActivateCrystal(TempleStatueType type)
    {
        switch (type)
        {
            case TempleStatueType.Blue:
                blueCrystal.SetActive(true);
                break;

            case TempleStatueType.Red:
                redCrystal.SetActive(true);
                break;

            case TempleStatueType.Green:
                greenCrystal.SetActive(true);
                break;

            case TempleStatueType.Yellow:
                yellowCrystal.SetActive(true);
                break;
        }
    }

    public void DisableDoor()
    {
        door.gameObject.SetActive(false);
    }
}
