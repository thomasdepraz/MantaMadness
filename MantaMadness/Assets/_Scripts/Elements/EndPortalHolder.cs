using FMODUnity;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class EndPortalHolder : MonoBehaviour, IDataPersistence
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineBlendDefinition blend;
    [SerializeField] private EventReference ClearSound;

    [SerializeField] private GameObject portalObject;
    bool activated;

    public void LoadData(GameData data)
    {
        StartCoroutine(DelayLoadData(data));
    }

    public IEnumerator DelayLoadData(GameData data)
    {
        yield return null;
        yield return null;

        if (data.endPortalActive)
        {
            EnableEndPortal();
        }
        else
        {
            DisableEndPortal();
        }
    }

    public void SaveData(ref GameData data)
    {
        data.endPortalActive = activated;
    }

    public void CouroutinSpawnStart(SimpleController controller)
    {
        StartCoroutine(SpawnTeleporterProcess(controller));
    }

    public IEnumerator SpawnTeleporterProcess(SimpleController controller)
    {
        // PART 1 LOCK PLAYER ACTIVATE CAM
        //lock player
        controller.ForceLock(true);
        controller.RailLock(true);

        //activate camera + play sound
        vcam.enabled = true;
        CameraManager.Instance.BlendToCamera(vcam, blend);

        yield return new WaitForSeconds(2f);

        //PART 2 SPAWN IN PORTAL
        RuntimeManager.PlayOneShot(ClearSound, vcam.transform.position);


        if (activated == false)
        {
            //coin.transform.localScale = Vector3.zero;
            portalObject.SetActive(true);
            activated = true;
        }

        yield return new WaitForSeconds(3f);
        //PART 3 RESET TO DEFAULT
        //unlock player
        controller.ForceLock(false);
        controller.RailLock(false);


        //reset camera
        CameraManager.Instance.ResetCamera(vcam);
        vcam.enabled = false;
    }

    private void EnableEndPortal()
    {
        portalObject.SetActive(true);
        activated = true;
    }

    private void DisableEndPortal()
    {
        portalObject.SetActive(false);
        activated = false;
    }
}
