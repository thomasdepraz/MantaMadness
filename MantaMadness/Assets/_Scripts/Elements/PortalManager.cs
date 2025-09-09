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

    public IEnumerator Teleport(string targetIndex, bool secretRoomMusic)
    {
        // Set Velocity to 0
        Game.Instance.player?.LockPlayerForDuration(teleportTransitionDuration);

        UIManager.Instance.transitionScreen.TransitionIn();
        FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Warping, 1);
        yield return new WaitForSeconds(teleportTransitionDuration/2);
        for(int i = 0; i < portals.Length; i++)
        {
            if (portals[i].index == targetIndex)
            {
                portals[i].Teleport();
                break;
            }
        }
        if (secretRoomMusic)
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_SecretRoom, 1f);
        }
        else if (!secretRoomMusic)
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_SecretRoom, 0f);
        }
        yield return new WaitForSeconds(teleportTransitionDuration / 2);
        FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Warping, 0);
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
