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

    [SerializeField] private UnityEngine.GameObject portalObject;
    //bool activated; 
    public int endPortalActivationState = 0;

    private SimpleController player;

    public void LoadData(GameData data)
    {
        StartCoroutine(DelayLoadData(data));
    }

    public IEnumerator DelayLoadData(GameData data)
    {
        yield return null;
        yield return null;
        player = Game.Instance.player;

        endPortalActivationState = data.endPortalState;
        Debug.Log("END PORTAL STATE" + endPortalActivationState);

        if (data.endPortalState == 2)
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
        data.endPortalState = endPortalActivationState;
    }

    public void EnableEndPortalStateActive()
    {
        endPortalActivationState = 1;
        if (WorldLevelManager.Instance.currentLevel == "WORLD")
        {
            StartCoroutine(SpawnTeleporterProcess());
        }
    }

    public void SpecialEnableEndPortal()
    {
        if(endPortalActivationState == 1)
        {
            StartCoroutine (SpawnTeleporterProcess());
        }
    }

    public IEnumerator SpawnTeleporterProcess()
    {
        // PART 1 LOCK PLAYER ACTIVATE CAM
        //lock player
        player.ForceLock(true);
        player.RailLock(true);

        //activate camera + play sound
        vcam.enabled = true;
        CameraManager.Instance.BlendToCamera(vcam, blend);

        yield return new WaitForSeconds(2f);

        //PART 2 SPAWN IN PORTAL
        RuntimeManager.PlayOneShot(ClearSound, vcam.transform.position);


        if (endPortalActivationState != 2)
        {
            //coin.transform.localScale = Vector3.zero;
            portalObject.SetActive(true);
            endPortalActivationState = 2;
        }

        yield return new WaitForSeconds(3f);
        //PART 3 RESET TO DEFAULT
        //unlock player
        player.ForceLock(false);
        player.RailLock(false);


        //reset camera
        CameraManager.Instance.ResetCamera(vcam);
        vcam.enabled = false;

        DialogManager.instance.StartCinematicInteraction(Game.Instance.goBackNpc);
    }

    private void EnableEndPortal()
    {
        portalObject.SetActive(true);
        endPortalActivationState = 2;
    }

    private void DisableEndPortal()
    {
        portalObject.SetActive(false);
        endPortalActivationState = 0;
    }
}
