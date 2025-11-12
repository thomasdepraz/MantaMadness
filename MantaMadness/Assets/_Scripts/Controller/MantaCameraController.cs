using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class MantaCameraController : MonoBehaviour
{
    public static MantaCameraController instance;

    public float fallingSpeedThreshold;
    public float airRideSpeedThreshold;

    public CinemachineCamera surfingCamera;
    public CinemachineCamera divingCamera;
    public CinemachineCamera airRideCamera;
    public CinemachineCamera swimmingCamera;
    public CinemachineCamera fallingCamera;
    public CinemachineCamera jumpingCamera;
    public CinemachineCamera railCamera;
    public CinemachineCamera bubbleCanonCamera;
    public CinemachineCamera stompCamera;

    private SimpleController mantaController;

    [Header("Surfing rotation offset")]
    public Vector2 minMaxPosition;
    public float rotationSpeed = 5;
    private CinemachineRotationComposer surfingCameraRotationComposer;


    List<CinemachineCamera> cameras = new List<CinemachineCamera>();

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        mantaController = GetComponent<SimpleController>();
        mantaController.stateChanged += UpdateState;
        mantaController.enterAirRail += EnterAirRail;
        mantaController.exitAirRail += ExitAirRail;
        mantaController.enterRail += EnterRail;
        mantaController.exitRail += ExitRail;

        cameras.Add(surfingCamera);
        cameras.Add(divingCamera);
        cameras.Add(airRideCamera);
        cameras.Add(swimmingCamera);
        cameras.Add(fallingCamera);
        cameras.Add(jumpingCamera);
        cameras.Add(railCamera);
        cameras.Add(bubbleCanonCamera);
        cameras.Add(stompCamera);

        surfingCameraRotationComposer = surfingCamera.gameObject.GetComponent<CinemachineRotationComposer>();
    }

    private void Start()
    {
        CameraManager.Instance.SetDefaultCamera(surfingCamera);
        SetActiveCamera(surfingCamera);
    }

    private void Update()
    {
        if (mantaController.State == ControllerState.SURFING)
        {
            float t =  math.remap(5, -5, 0, 1, mantaController.AngularVelocity.y);
            float target = Mathf.Lerp(minMaxPosition.x, minMaxPosition.y, t);

            //surfingCameraRotationComposer.Composition.ScreenPosition.x = Mathf.Lerp(surfingCameraRotationComposer.Composition.ScreenPosition.x, target, Time.deltaTime * rotationSpeed);
        }
    }

    private void EnterAirRail(AirRail rail)
    {
        rail.rideCamera.Target.TrackingTarget = transform;
        rail.rideCamera.Target.LookAtTarget = transform;
        rail.rideCamera.gameObject.SetActive(true);
        rail.rideCamera.enabled = true;
    }

    private void ExitAirRail(AirRail rail)
    {
        rail.rideCamera.gameObject.SetActive(false);
    }

    private void EnterRail()
    {
        SetActiveCamera(railCamera);
    }

    private void ExitRail()
    {
        UpdateState(mantaController.State, mantaController.State);
    }

    private void UpdateState(ControllerState previousState, ControllerState newState)
    {
        //switch (newState)
        //{
        //    case ControllerState.FALLING:
        //        SetActiveCamera(surfingCamera);
        //        break;
        //    case ControllerState.JUMPING:
        //        SetActiveCamera(surfingCamera);
        //        break;
        //    case ControllerState.SURFING:
        //        SetActiveCamera(surfingCamera);
        //        break;
        //    case ControllerState.DIVING:
        //        SetActiveCamera(surfingCamera);
        //        break;
        //    case ControllerState.SWIMMING:
        //        SetActiveCamera(surfingCamera);
        //        break;
        //    default:
        //        break;
        //}

        switch (newState)
        {
            case ControllerState.STOMP:
                SetActiveCamera(stompCamera);
                break;
            default:
                SetActiveCamera(surfingCamera);
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

    public void DeactivatePlayerCamera()
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            cameras[i].gameObject.SetActive(false);
        }
    }
    public void ActivatePlayerCamera()
    {
        SetActiveCamera(surfingCamera);
    }

}