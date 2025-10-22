using System.Collections;
using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    public static HitStopManager instance;
    private CameraTargetController cameraController;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        cameraController = CameraTargetController.instance;
    }

    private Coroutine stopRoutine;
    public void Stop(float duration)
    {
        if(stopRoutine != null)
        {
            stopRoutine = null;
            stopRoutine = StartCoroutine(Wait(duration));
        }
        else
        {
            stopRoutine = StartCoroutine(Wait(duration));
        }
    }

    IEnumerator Wait(float duration)
    {
        cameraController.enabled = false;
        Game.Instance.isHitStop = true;
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1.0f;
        Game.Instance.isHitStop = false;
        cameraController.enabled = true;
    }
}
