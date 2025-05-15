using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Coin : MonoBehaviour
{
    const float c_LockDuration = 3f;

    [Header("Cinemachine")]
    public CinemachineCamera vcamera;
    public CinemachineBlendDefinition blend;

    private WaitForSeconds wait;
    private IEnumerator PickupCoroutine(SimpleController controller)
    {
        wait = new WaitForSeconds(c_LockDuration);

        //lock player
        controller.ForceLock(true);

        //activate camera
        CameraManager.Instance.BlendToCamera(vcamera, blend);   
        
        //animation
        
        yield return wait;

        //unlock player
        controller.ForceLock(false);

        //reset camera
        CameraManager.Instance.ResetCamera(vcamera);

        //increase coin count
        CoinManager.Instance.PickupCoin();

        //Deactivate game object
        routine = null;
        Destroy(gameObject);

    }

    Coroutine routine;
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out SimpleController controller) && routine == null)
        {
            routine = StartCoroutine(PickupCoroutine(controller));
        }
    }
}
