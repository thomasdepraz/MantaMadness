using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum PlayerActionFMOD
{
    BOOST,
    JUMP,
    SPLASH,
    STYLE,
    SURF
}

public class PlayerActionFMODManager : MonoBehaviour
{
    [HideInInspector]public static PlayerActionFMODManager Instance;

    [SerializeField] private EventReference[] playerFmodActions; 

    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }


    public void PlayPlayerAction(PlayerActionFMOD actionName)
    {
        RuntimeManager.PlayOneShot(playerFmodActions[(int)actionName], Game.Instance.player.transform.position);
    }

    public void PlayStyleAction(PlayerActionFMOD actionName, int State)
    {
        EventInstance instance = RuntimeManager.CreateInstance(playerFmodActions[(int)actionName]);
        FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_StyleState, State);

        instance.start();
        instance.release();
    }
}
