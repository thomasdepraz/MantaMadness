using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance;
    public Portal[] portals;

    private float teleportTransitionDuration = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
    }

    public IEnumerator Teleport(string targetIndex)
    {
        // Set Velocity to 0
        Game.Instance.player?.LockPlayerForDuration(teleportTransitionDuration);

        UIManager.Instance.transitionScreen.TransitionIn();
        yield return new WaitForSeconds(teleportTransitionDuration/2);
        for(int i = 0; i < portals.Length; i++)
        {
            if (portals[i].index == targetIndex)
            {
                portals[i].Teleport();
                break;
            }
        }
        yield return new WaitForSeconds(teleportTransitionDuration / 2);
        UIManager.Instance.transitionScreen.TransitionOut();
        yield return null;
    }

    public void SetCheckpoint(string index, bool areaName, string nameToDisplay)
    {
        Transform respawnPos = transform;

        for (int i = 0; i < portals.Length; i++)
        {
            if (portals[i].index == index)
            {
                respawnPos = portals[i].teleportPoint;
                break;
            }
        }

        WorldCheckpointManager.Instance.SetCheckpoint(respawnPos, index, areaName, nameToDisplay);
    }

}
