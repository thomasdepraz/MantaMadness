using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class BackroomPortal : Portal
{
    [SerializeField] protected AMBIENT ambientToPlay = AMBIENT.NULL;
    private Coroutine cooldownCoroutine = null;

    private SteamSignal signal;

    protected override void Start()
    {
        base.Start();

        if (player == null)
        {
            player = Game.Instance.player;
        }

        if (signText != null)
        {
            signText.text = indexName;
        }

        if (!PortalManager.Instance.portals.Contains(this))
        {
            PortalManager.Instance.portals.Add(this);
        }
        signal = GetComponent<SteamSignal>();
        }

    public override void Teleport()
    {
        player.transform.position = teleportPoint.position;
        player.transform.rotation = new Quaternion(0, teleportPoint.transform.rotation.y, 0, teleportPoint.transform.rotation.w);

        if (CameraTargetController.instance != null)
            CameraTargetController.instance.SyncYawPitchToPlayerFacing();


        //if (!string.IsNullOrEmpty(collectibleAreaID))
        //{
        //    CollectibleAreaRegistry.Instance.SetCurrentArea(collectibleAreaID);
        //}
    }

    protected override void OnTriggerEnter(Collider other)
    {
        //RIEN MON GARS RIEN
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if (controller.State == ControllerState.STOMP)
            {
                if (cooldownCoroutine == null)
                {
                    cooldownCoroutine = StartCoroutine(CooldownRoutine(controller));
                    controller.ForceLock(true);
                    PortalManager.Instance.SetCheckpoint(targetIndex, displayAreaName, nameToDisplay, LevelID, collectibleAreaID);
                    PortalManager.Instance.StartCoroutine(PortalManager.Instance.BackroomTeleport(targetIndex, enterSecretRoom, musicToPlay,  specialWeatherType, ambientToPlay, areaIntro, this));

                    if (signal != null)
                    {
                        signal.Trigger();
                    }
                }
            }
        }
    }

    private IEnumerator CooldownRoutine(SimpleController player)
    {
        yield return new WaitForSeconds(0.5f);
        player.ForceLock(false);
        cooldownCoroutine = null;
    }

    public override void EnableMat()
    {
        //RIEN
    }

    public override void DisableMat()
    {
        //RIEN
    }
}
