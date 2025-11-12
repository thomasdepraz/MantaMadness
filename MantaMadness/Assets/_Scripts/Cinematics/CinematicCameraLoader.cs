using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CinematicCameraLoader : MonoBehaviour
{
    private CinemachineCamera cam;
    private void Awake()
    {
        cam = GetComponent<CinemachineCamera>();
    }

    private void Start()
    {
        CinematicManager.instance.cinematicCameras.Add(cam);
        CinematicManager.instance.ResetCam();
    }
}
