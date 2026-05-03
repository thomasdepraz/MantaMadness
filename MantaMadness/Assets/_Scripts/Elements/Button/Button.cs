using DG.Tweening;
using FMODUnity;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public interface IButtonAction
{
    void Execute();
}

public class Button : MonoBehaviour
{
    [Header("Parameter")]
    public float spawnTime = 0.05f;

    [Header("Cinemachine")]
    public CinemachineCamera vcam;
    public CinemachineBlendDefinition blend;

    protected WaitForSeconds wait;
    protected Coroutine routine;

    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;

    protected const float c_lockDuration = 4f;
    protected bool isActivated = false;

    public MeshRenderer buttonMesh;
    public Material activatedMaterial;

    public EventReference buttonStinger;

    [SerializeField] private MonoBehaviour[] actionsTargets;
    private IButtonAction[] actions;


    protected virtual void Start()
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

        actions = new IButtonAction[actionsTargets.Length];

        for (int i = 0; i < actionsTargets.Length; i++)
        {
            actions[i] = actionsTargets[i] as IButtonAction;

            if (actions[i] == null)
            {
                Debug.LogError(actionsTargets[i].name + " does not implement IButtonAction");
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if(isActivated == false && routine == null)
            routine = StartCoroutine(ActivationCoroutine(controller));
        }
    }

    protected virtual IEnumerator ActivationCoroutine(SimpleController controller)
    {
        isActivated = true;
        buttonMesh.material = activatedMaterial;

        RuntimeManager.PlayOneShot(buttonStinger, Camera.main.transform.position);

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
        //UIManager.Instance.transitionScreen.TransitionInOut();
        //yield return new WaitForSeconds(0.5f);

        // PART 2 SPAWN IN OBJECTS

        //foreach (GameObject objects in objectsToActivate)
        //{
        //    objects.SetActive(true);
        //}

        foreach (var action in actions)
        {
            action?.Execute();
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
