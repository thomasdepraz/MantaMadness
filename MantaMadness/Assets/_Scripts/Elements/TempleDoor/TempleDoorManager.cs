using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Timeline;

public class TempleDoorManager : SpecialDestructibleManager
{
    [SerializeField] private List<TimelineAsset> statueCinematics = new();
    [SerializeField] private TimelineAsset doorOpeningCinematic;

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
        switch (statueType)
        {
            case TempleStatueType.Blue:
                //yield return new WaitForSeconds((float)blueCinematic.duration);
                break;
            case TempleStatueType.Yellow:
                break;
            case TempleStatueType.Green:
                break;
            case TempleStatueType.Red:
                break;
        }

        yield return new WaitForSeconds((float)statueCinematics[index].duration);

        if (destroyedCount >= destructibles.Count)
        {
            //yield return doorOpeningCinematic.Play();
        }
    }

}
