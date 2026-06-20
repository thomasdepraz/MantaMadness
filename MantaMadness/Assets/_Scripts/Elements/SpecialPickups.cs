using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpecialPickups : MonoBehaviour, IDataPersistence
{
    [SerializeField] private CollectibleRelay relay;
    [SerializeField] private string specialPickupName;

    [SerializeField] private GameObject[] visuals;

    [SerializeField] private int updateGameStateValue;
    [SerializeField] private float speed = 0.8f;

    private GameObject player;

    [Header("FMOD Sound")]
    public EventReference pickupSound;

    [Header("Particle")]
    public UiParticles particle;

    private bool hasBeenObtained;

    private bool movingTowardtarget = false;

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
            relay.HitCollision += MoveToTarget;
        }

        if (hasBeenObtained == true)
        {
            DisablePickup();
        }
    }

    private void OnDisable()
    {
        if (relay != null)
        {
            relay.HitCollision -= MoveToTarget;
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            OnPickup(controller);
        }
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

        //Update save for door check
        GameData data = DataPersistenceManager.Instance.gameData;

        if (data.specialPickups.ContainsKey(specialPickupName))
            data.specialPickups[specialPickupName] = true;
        else
            data.specialPickups.Add(specialPickupName, true);

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
        relay.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        if (hasBeenObtained) return;

        if (movingTowardtarget == true && player != null)
        {
            transform.position = Vector3.Lerp(transform.position, player.transform.position, speed);
        }
    }

    public void MoveToTarget(GameObject target)
    {
        if (movingTowardtarget == false)
        {
            player = target;
            movingTowardtarget = true;
        }
    }
}
