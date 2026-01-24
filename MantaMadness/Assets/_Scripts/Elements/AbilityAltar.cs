using Unity.Cinemachine;
using UnityEngine;
using System;
using System.Collections;
using TMPro;
using FMOD.Studio;
using FMODUnity;

public class AbilityAltar : MonoBehaviour, IDataPersistence
{
    [SerializeField] private CollisionRelay relay;
    [SerializeField] private Transform playerPoint;
    [SerializeField] private CinemachineCamera cam;
    [SerializeField] private string abilityName;
    //[SerializeField] private ControllerAbility abilityName;
    [SerializeField] private string dialogSequenceName;

    [SerializeField] private GameObject[] visuals;

    [Header("FMOD Sound")]
    public EventReference pickupAltarSound;

    private bool hasBeenObtained;

    [SerializeField] private int updateGameStateValue;

    private void Start()
    {
        StartCoroutine(StartDelay());
    }

    private IEnumerator StartDelay()
    {
        yield return new WaitForSeconds(0.1f);
        if (relay != null)
        {
            relay.HitCollision += OnPickup;
        }

        if (hasBeenObtained == true)
        {
            DisablePickup();
        }
    }

    private void OnDisable()
    {
        relay.HitCollision -= OnPickup;
    }

    public void LoadData(GameData data)
    {
        data.abilityAltars.TryGetValue(abilityName, out hasBeenObtained);
    }

    public void SaveData(ref GameData data)
    {
        if (data.abilityAltars.ContainsKey(abilityName))
        {
            data.abilityAltars.Remove(abilityName);
        }
        data.abilityAltars.Add(abilityName, hasBeenObtained);
    }

    private void OnPickup(SimpleController player)
    {
        //if (Game.Instance.player.State != ControllerState.SURFING)
        //    return;

        RuntimeManager.PlayOneShot(pickupAltarSound, Game.Instance.player.transform.position);

        string[] abilityTypeNames = Enum.GetNames(typeof(ControllerAbility));

        if(updateGameStateValue > 0)
        {
            Game.Instance.SetGameState(updateGameStateValue);
        }

        for(int i = 0; i < abilityTypeNames.Length; i++)
        {
            if (abilityTypeNames[i] == abilityName)
            {
                player.UnlockAbility(abilityTypeNames[i]);
            }
        }
        hasBeenObtained = true;

        //player.transform.position = playerPoint.transform.position;
        player.ForcePosition(playerPoint.position,playerPoint.rotation,resetVelocity: true,forcedState: ControllerState.SURFING);
        DisablePickup();

        if (dialogSequenceName != null)
        DialogManager.instance.StartSequence(dialogSequenceName);


        // Unlock player ability => set the right boolean
        // Lock player
        // Place player on player point
        // Activate altar cam
        // Activate particles
        // Deactivate some visuals
        // Show UI with indications
        // Wait for X seconds // IDEALLY Link to dialog system when it's done
        // unlock player
    }

    private void DisablePickup()
    {
        foreach(GameObject visual in visuals)
        {
            visual.SetActive(false);
        }
        relay.HitCollision -= OnPickup;
        relay.gameObject.SetActive(false);
    }
}
