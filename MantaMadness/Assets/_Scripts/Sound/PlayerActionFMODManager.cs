using UnityEngine;
using FMODUnity;
using System;

public enum PlayerActionFMOD
{
    BOOST,
    JUMP,
    SPLASH
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


}
