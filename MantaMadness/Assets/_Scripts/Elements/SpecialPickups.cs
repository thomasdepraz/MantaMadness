using FMODUnity;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SpecialPickups : MonoBehaviour, IDataPersistence
{
    [SerializeField] private CollisionRelay relay;
    [SerializeField] private string specialPickupName;

    [SerializeField] private GameObject[] visuals;

    [SerializeField] private int updateGameStateValue;

    [Header("FMOD Sound")]
    public EventReference pickupSound;

    [Header("Particle")]
    public UiWordsParticles particle;

    private bool hasBeenObtained;

    private void Start()
    {
        StartCoroutine(StartDelay());
        print("hasbeenobtained" + hasBeenObtained);
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
        data.specialPickups.TryGetValue(specialPickupName, out hasBeenObtained);
    }

    public void SaveData(ref GameData data)
    {
        if (data.specialPickups.ContainsKey(specialPickupName))
        {
            data.specialPickups.Remove(specialPickupName);
        }
        data.specialPickups.Add(specialPickupName, hasBeenObtained);
    }

    private void OnPickup(SimpleController player)
    {

        RuntimeManager.PlayOneShot(pickupSound, Game.Instance.player.transform.position);

        StartCoroutine(VFXSequence());
        if (updateGameStateValue > 0)
        {
            Game.Instance.SetGameState(updateGameStateValue);
        }

        hasBeenObtained = true;

        DisablePickup();
    }

    private IEnumerator VFXSequence()
    {
        //VFX SEQUENCE
        UIManager.Instance.gameInterface.pickupSpecialItem(particle);
        yield return null;
    }
    private void DisablePickup()
    {
        foreach (GameObject visual in visuals)
        {
            visual.SetActive(false);
        }
        relay.HitCollision -= OnPickup;
        relay.gameObject.SetActive(false);
    }
}
