using System;
using Unity.Cinemachine;
using UnityEngine;

public class MantaCameraController : MonoBehaviour
{
    public float fallingSpeedThreshold;
    public float airRideSpeedThreshold;
    public CinemachineCamera followCamera;
    public CinemachineCamera divingCamera;
    public CinemachineCamera airRideCamera;

    private SimpleController mantaController;

    private void Awake()
    {
        mantaController = GetComponent<SimpleController>();
        mantaController.stateChanged += UpdateState;
        mantaController.enterAirRail += EnterRail;
        mantaController.exitAirRail += ExitRail;
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
        if(newState == ControllerState.SURFING)
        {
            followCamera.gameObject.SetActive(true);
            airRideCamera.gameObject.SetActive(false);
            divingCamera.gameObject.SetActive(false);
        }

        if (newState == ControllerState.AIRRIDE)
        {
            followCamera.gameObject.SetActive(false);
            divingCamera.gameObject.SetActive(false);
            airRideCamera.gameObject.SetActive(true);
            return;
        }

        if (newState == ControllerState.DIVING && previousState != ControllerState.SURFING)
        {
            followCamera.gameObject.SetActive(false);
            divingCamera.gameObject.SetActive(true);
            airRideCamera.gameObject.SetActive(false);
            return;
        }

        if (previousState == ControllerState.AIRRIDE)
        {
            followCamera.gameObject.SetActive(true);
            airRideCamera.gameObject.SetActive(false);
        }
    }

}