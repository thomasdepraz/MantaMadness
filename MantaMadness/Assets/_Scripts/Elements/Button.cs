using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class Button : MonoBehaviour
{
    [Header("Cinemachine")]
    public CinemachineCamera vcam;
    public CinemachineBlendDefinition blend;

    private WaitForSeconds wait;
    private Coroutine routine;

    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;

    private const float c_lockDuration = 4f;
    private bool isActivated = false;

    public MeshRenderer buttonMesh;
    public Material activatedMaterial;

    private void Start()
    {
        if(isActivated == false)
        {
            foreach (GameObject objects in objectsToActivate)
            {
                objects.SetActive(false);
            }

            foreach (GameObject objects in objectsToDeactivate)
            {
                objects.SetActive(true);
            }
        }

        vcam.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if(isActivated == false && routine == null)
            routine = StartCoroutine(ActivationCoroutine(controller));
        }
    }

    private IEnumerator ActivationCoroutine(SimpleController controller)
    {
        isActivated = true;
        buttonMesh.material = activatedMaterial;

        yield return new WaitForSeconds(0.75f);

        // PART 1 LOCK PLAYER ACTIVATE CAM
        //lock player
        controller.ForceLock(true);

        //activate camera

        vcam.enabled = true;
        CameraManager.Instance.BlendToCamera(vcam, blend);

        //increase boost gauge
        //Game.Instance.player.boostBehaviour.IncrementGauge(BoostAction.CoolSun);

        //animation

        yield return new WaitForSeconds(c_lockDuration / 2);
        UIManager.Instance.transitionScreen.TransitionInOut();
        yield return new WaitForSeconds(0.5f);

        // PART 2 SPAWN IN OBJECTS

        foreach (GameObject objects in objectsToActivate)
        {
            objects.SetActive(true);
        }

        foreach (GameObject objects in objectsToDeactivate)
        {
            objects.SetActive(false);
        }

        yield return new WaitForSeconds(c_lockDuration / 2);

        // PART 3 RESET BACK TO NORMAL

        //unlock player
        controller.ForceLock(false);

        //reset camera
        CameraManager.Instance.ResetCamera(vcam);

        routine = null;


    }
}
