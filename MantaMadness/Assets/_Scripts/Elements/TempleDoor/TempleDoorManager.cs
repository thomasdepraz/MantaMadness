using DG.Tweening;
using FMOD.Studio;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class TempleDoorManager : SpecialDestructibleManager, IDataPersistence
{
    [SerializeField] private CinemachineCamera cinematicCamera;
    [SerializeField] private CinemachineBlendDefinition blend;

    [SerializeField] private Transform blueCamPoint;
    [SerializeField] private Transform redCamPoint;
    [SerializeField] private Transform yellowCamPoint;
    [SerializeField] private Transform greenCamPoint;
    [SerializeField] private Transform doorCamPoint;
    [SerializeField] private Transform doorTrackerPoint;
    [SerializeField] private float camMoveDuration = 1.2f;
    [SerializeField] private float crystalDelay = 0.3f;
    [SerializeField] private float endDelay = 1.2f;

    [SerializeField] private GameObject blueCrystal;
    [SerializeField] private GameObject redCrystal;
    [SerializeField] private GameObject yellowCrystal;
    [SerializeField] private GameObject greenCrystal;
    [SerializeField] private GameObject blueDisabledCrystal;
    [SerializeField] private GameObject redDisabledCrystal;
    [SerializeField] private GameObject yellowDisabledCrystal;
    [SerializeField] private GameObject greenDisabledCrystal;

    [SerializeField] private GameObject door;

    public void OnDestructibleDestroyed(TempleStatueDestructible destructible)
    {
        int index = destroyedCount - 1;
        StartCoroutine(PlayStatueCinematic(index, destructible.type));
    }

    protected IEnumerator PlayStatueCinematic(int index, TempleStatueType statueType)
    {
        Game.Instance.player.ForceLock(true);

        Transform targetPoint = GetCameraPoint(statueType);

        MantaCameraController.instance.DeactivatePlayerCamera();
        //cinematicCamera.gameObject.SetActive(true);
        CameraManager.Instance.BlendToCamera(cinematicCamera, blend);
        cinematicCamera.LookAt = SetTrackingTarget(statueType);

        yield return MoveCameraToPoint(targetPoint, camMoveDuration);

        yield return new WaitForSeconds(crystalDelay);

        ActivateCrystal(statueType);

        yield return new WaitForSeconds(endDelay);

        if (destroyedCount >= destructibles.Count)
        {
            yield return DoorOpeningRoutine();
        }

        cinematicCamera.gameObject.SetActive(false);
        MantaCameraController.instance.ActivatePlayerCamera();

        Game.Instance.player.ForceLock(false);
    }

    private Transform GetCameraPoint(TempleStatueType type)
    {
        switch (type)
        {
            case TempleStatueType.Blue:
                return blueCamPoint;

            case TempleStatueType.Red:
                return redCamPoint;

            case TempleStatueType.Yellow:
                return yellowCamPoint;

            case TempleStatueType.Green:
                return greenCamPoint;

            default:
                return blueCamPoint;
        }
    }

    private IEnumerator MoveCameraToPoint(Transform target, float duration)
    {
        Vector3 startPos = target.position;
        Quaternion startRot = cinematicCamera.transform.rotation;

        Vector3 endPos = target.position + Camera.main.transform.forward * 3f;
        //Quaternion endRot = target.rotation;

        float elapsed = 0f;

        //+set tracking target

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            t = Mathf.SmoothStep(0f, 1f, t);

            cinematicCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            //cinematicCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        cinematicCamera.transform.position = endPos;
        //cinematicCamera.transform.rotation = endRot;
    }

    private IEnumerator DoorOpeningRoutine()
    {

        Game.Instance.player.ForceLock(true);

        Transform targetPoint = doorCamPoint;
        CameraManager.Instance.BlendToCamera(cinematicCamera, blend);
        cinematicCamera.LookAt = doorTrackerPoint;



        yield return MoveCameraToPoint(targetPoint, camMoveDuration);
        door.transform.DOLocalMoveY(door.transform.localPosition.y - 50f, 2f);
        yield return new WaitForSeconds(2.5f);

        DisableDoor();

    }

    private Transform SetTrackingTarget(TempleStatueType type)
    {
        switch (type)
        {
            case TempleStatueType.Blue:
                return blueCrystal.transform;

            case TempleStatueType.Red:
                return redCrystal.transform;

            case TempleStatueType.Yellow:
                return yellowCrystal.transform;

            case TempleStatueType.Green:
                return greenCrystal.transform;

            default:
                return blueCrystal.transform;
        }
    }


    public void LoadData(GameData data)
    {
        destroyedCount = 0;

        foreach (var destructible in destructibles)
        {
            if (destructible is TempleStatueDestructible statue)
            {
                if (data.puzzleElements.TryGetValue(statue.id, out bool state) && state)
                {
                    destroyedCount++;
                    ActivateCrystal(statue.type);
                }
            }
        }

        if (destroyedCount >= destructibles.Count)
        {
            DisableDoor();
        }
    }

    public void SaveData(ref GameData data)
    {
        //throw new System.NotImplementedException();
    }

    public void ActivateCrystal(TempleStatueType type)
    {
        switch (type)
        {
            case TempleStatueType.Blue:
                blueCrystal.SetActive(true);
                blueDisabledCrystal.SetActive(false);
                break;

            case TempleStatueType.Red:
                redCrystal.SetActive(true);
                redDisabledCrystal.SetActive(false);
                break;

            case TempleStatueType.Green:
                greenCrystal.SetActive(true);
                greenDisabledCrystal.SetActive(false);
                break;

            case TempleStatueType.Yellow:
                yellowCrystal.SetActive(true);
                yellowDisabledCrystal.SetActive(false);
                break;
        }
    }

    public void DisableDoor()
    {
        door.gameObject.SetActive(false);
    }
}
