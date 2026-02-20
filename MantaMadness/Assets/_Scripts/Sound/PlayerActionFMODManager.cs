using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerActionFMOD
{
    BOOST,
    JUMP,
    SPLASH,
    STYLE,
    SURF,
    DRIFT,
    DEATH,
    CHARGEDBOOST,
    CHARGINGBOOST,
    GRINDRAIL,
    BUMP,
    FLY,
    CAT,
    EXPLOSION,
    STOMPJUMP,
}

public enum PlayerRailGrindType
{
    LIGHT,
    CONCRETE,
    WOOD,
    METAL
}

[Serializable]
public struct PlayerActionEventReferencePair
{
    public PlayerActionFMOD action;
    public EventReference eventReference;
    public bool isLooping;
}

public class PlayerActionFMODManager : MonoBehaviour
{
    [HideInInspector]public static PlayerActionFMODManager Instance;
    [SerializeField] private List<PlayerActionEventReferencePair> playerFmodActionsList = new();

    private readonly Dictionary<PlayerActionFMOD, EventInstance> loopingSounds = new();
    private SimpleController mantaController;


    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        mantaController = Game.Instance.player;
        mantaController.stateChanged += StateChanged;
    }

    private void OnDisable()
    {
        mantaController.stateChanged -= StateChanged;
    }

    public void StateChanged(ControllerState previousState, ControllerState newState)
    {
        if (newState == ControllerState.FALLING)
        {
            PlayPlayerAction(PlayerActionFMOD.FLY);
        }

        if (previousState == ControllerState.FALLING)
        {
            TryStopLoopingSound(PlayerActionFMOD.FLY);
        }
    }

    public void PlayPlayerAction(PlayerActionFMOD actionName)
    {
        EventReference eventReference = GetEventReference(actionName, out bool isLooping);

        if(false == isLooping)
        {
            RuntimeManager.PlayOneShot(eventReference, Game.Instance.player.transform.position);
            return;
        }

        //Handle looping sounds
        if (loopingSounds.ContainsKey(actionName))
        {
            //stop current before 
            loopingSounds[actionName].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            loopingSounds.Remove(actionName);
        }

        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        loopingSounds[actionName] = eventInstance;
        eventInstance.start();
    }

    public void PlayPlayerActionWithParam(PlayerActionFMOD actionName,string parameterName, float paramValue)
    {
        EventReference eventReference = GetEventReference(actionName, out bool isLooping);


        if (!isLooping)
        {
            EventInstance instance = RuntimeManager.CreateInstance(eventReference);
            instance.setParameterByName(parameterName, paramValue);
            instance.set3DAttributes(RuntimeUtils.To3DAttributes(Game.Instance.player.transform.position));
            instance.start();
            instance.release();
            return;
        }

        if (loopingSounds.ContainsKey(actionName))
        {
            loopingSounds[actionName].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            loopingSounds.Remove(actionName);
        }

        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstance.setParameterByName(parameterName, paramValue);
        eventInstance.start();

        loopingSounds[actionName] = eventInstance;
    }


    public bool TryStopLoopingSound(PlayerActionFMOD action)
    {
        if (loopingSounds.ContainsKey(action))
        {
            //stop current looping sound
            loopingSounds[action].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            loopingSounds.Remove(action);
            return true;
        }

        return false;
    }

    private EventReference GetEventReference(PlayerActionFMOD action, out bool isLooping)
    {
        isLooping = false;
        foreach (var item in playerFmodActionsList)
        { 
            if(item.action == action)
            {
                isLooping = item.isLooping;
                return item.eventReference;
            }
        }
        Debug.LogError($"Found no matching event reference for action : {action}");
        return default;
    }

    public void PlayStyleAction(PlayerActionFMOD actionName, int State)
    {
        EventInstance instance = RuntimeManager.CreateInstance(GetEventReference(actionName, out _));
        FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_StyleState, State);

        instance.start();
        instance.release();
    }
}
