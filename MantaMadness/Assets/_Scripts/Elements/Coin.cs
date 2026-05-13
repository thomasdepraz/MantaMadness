using FMODUnity;
using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Coin : MonoBehaviour
{
    const float c_LockDuration = 3f;

    [Header("FMOD Sound")]
    public EventReference pickupSound;

    [Header("Cinemachine")]
    public CinemachineCamera vcamera;
    public CinemachineBlendDefinition blend;

    [Header("Saving")]
    public string saveName;
    public bool isMiniGameCoin;

    private WaitForSeconds wait;
    private Coroutine routine;

    public Action<bool> pickedUpCoin;
    private IEnumerator PickupCoroutine(SimpleController controller)
    {
        wait = new WaitForSeconds(c_LockDuration);

        //lock player
        controller.ForceLock(true);

        //activate camera
        CameraManager.Instance.BlendToCamera(vcamera, blend);

        // Sound
        RuntimeManager.PlayOneShot(pickupSound, transform.position);

        //increase boost gauge
        //Game.Instance.player.boostBehaviour.IncrementGauge(BoostAction.CoolSun);

        //animation

        //
        UIManager.Instance.gameInterface.pickupJohnnyParticle();

        yield return wait;

        //unlock player
        controller.ForceLock(false);

        //reset camera
        CameraManager.Instance.ResetCamera(vcamera);

        //increase coin count
        CoinManager.Instance.PickupCoin();


        //Deactivate game object
        routine = null;
        pickedUpCoin.Invoke(true);

        //Update L'UI
        UIManager.Instance.gameInterface.RefreshAreaSunCount();

        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out SimpleController controller) && routine == null)
        {
            routine = StartCoroutine(PickupCoroutine(controller));
        }
    }
}
