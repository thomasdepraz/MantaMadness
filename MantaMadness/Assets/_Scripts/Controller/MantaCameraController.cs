using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class MantaCameraController : MonoBehaviour
{
    public float fallingSpeedThreshold;
    public float airRideSpeedThreshold;

    public CinemachineCamera surfingCamera;
    public CinemachineCamera divingCamera;
    public CinemachineCamera airRideCamera;
    public CinemachineCamera swimmingCamera;
    public CinemachineCamera fallingCamera;
    public CinemachineCamera jumpingCamera;

    private SimpleController mantaController;

    List<CinemachineCamera> cameras = new List<CinemachineCamera>();

    private void Awake()
    {
        mantaController = GetComponent<SimpleController>();
        mantaController.stateChanged += UpdateState;
        mantaController.enterAirRail += EnterRail;
        mantaController.exitAirRail += ExitRail;

        cameras.Add(surfingCamera);
        cameras.Add(divingCamera);
        cameras.Add(airRideCamera);
        cameras.Add(swimmingCamera);
        cameras.Add(fallingCamera);
        cameras.Add(jumpingCamera);
    }

    private void Start()
    {
        CameraManager.Instance.SetDefaultCamera(surfingCamera);
        SetActiveCamera(fallingCamera);
    }

    private void EnterRail(AirRail rail)
    {
        rail.rideCamera.Target.TrackingTarget = transform;
        rail.rideCamera.Target.LookAtTarget = transform;
        rail.rideCamera.gameObject.SetActive(true);
    }

    private void ExitRail(AirRail rail)
    {
        rail.rideCamera.gameObject.SetActive(false);
    }

    private void UpdateState(ControllerState previousState, ControllerState newState)
    {
        switch (newState)
        {
            case ControllerState.FALLING:
                SetActiveCamera(fallingCamera);
                break;
            case ControllerState.JUMPING:
                SetActiveCamera(jumpingCamera);
                break;
            case ControllerState.SURFING:
                SetActiveCamera(surfingCamera);
                break;
            case ControllerState.DIVING:
                SetActiveCamera(divingCamera);
                break;
            case ControllerState.SWIMMING:
                SetActiveCamera(swimmingCamera);
                break;
            default:
                break;
        }
    }

    private void SetActiveCamera(CinemachineCamera camera)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i].name == camera.name)
            {
                cameras[i].gameObject.SetActive(true);
            }
            else
            {
                cameras[i].gameObject.SetActive(false);
            }
        }
    }
}