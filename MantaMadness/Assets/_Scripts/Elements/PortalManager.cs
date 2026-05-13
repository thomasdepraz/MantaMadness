using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance;
    public List<Portal> portals;

    private float teleportTransitionDuration = 1.5f;

    [SerializeField] private EventReference warpInteract;

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

    public IEnumerator Teleport(string targetIndex, bool secretRoomMusic, MUSICS musicToPlay, Portal portal, WeatherType specialWeatherType)
    {
        // Set Velocity to 0
        Game.Instance.player?.LockPlayerForDuration(teleportTransitionDuration);
        PlayTeleportSFX("WarpState", 0);
        yield return null;

        UIManager.Instance.transitionScreen.TransitionInOut();
        FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Warping, 1);
        MusicManager.Instance.PlayMusic(musicToPlay);
        WeatherManager.instance.SetNewWeather(specialWeatherType);

        if (!string.IsNullOrEmpty(portal.collectibleAreaID))
        {
            CollectibleAreaRegistry.Instance.SetCurrentArea(portal.collectibleAreaID);
        }

        yield return new WaitForSeconds(teleportTransitionDuration/2);
        PlayTeleportSFX("WarpState", 1);
        for (int i = 0; i < portals.Count; i++)
        {
            if (portals[i].indexName == targetIndex)
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

    public void SetCheckpoint(string index, bool areaName, string nameToDisplay, string levelID)
    {
        Transform respawnPos = transform;

        for (int i = 0; i < portals.Count; i++)
        {
            if (portals[i].indexName == index)
            {
                respawnPos = portals[i].teleportPoint;
                break;
            }
        }

        WorldCheckpointManager.Instance.SetCheckpoint(respawnPos, index, areaName, nameToDisplay, levelID);
    }

    public void PlayTeleportSFX(string parameterName, float paramValue)
    {
        EventReference eventReference = warpInteract;

        EventInstance instance = RuntimeManager.CreateInstance(eventReference);
        instance.setParameterByName(parameterName, paramValue);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(Camera.main.transform.position));
        instance.start();
        instance.release();
        return;
    }

}
