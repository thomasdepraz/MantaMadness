using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraControllerData", menuName = "Game Data/CameraControllerData")]
[Serializable]
public class CameraControllerData : ScriptableObject
{
    
    [Header("Surfing Camera Parameters")]
    public float surf_sensitivity = 100f;
    public float surf_sensitivity_controller = 1000f;
    public float surf_minPitch = -45f;
    public float surf_maxPitch = 45f;
    public float surf_minYaw = -45f;
    public float surf_maxYaw = 45f;
    public float surf_smooth = 10f;

    [Header("Idle Camera Parameters")]
    public float idle_sensitivity = 100f;
    public float idle_sensitivity_controller = 1000f;
    public float idle_minPitch = -45f;
    public float idle_maxPitch = 45f;
    public float idle_minYaw = -45f;
    public float idle_maxYaw = 45f;
    public float idle_smooth = 10f;

    [Header("Swimming Camera Parameters")]
    public float swim_sensitivity = 100f;
    public float swim_sensitivity_controller = 1000f;
    public float swim_minPitch = -45f;
    public float swim_maxPitch = 45f;
    public float swim_minYaw = -45f;
    public float swim_maxYaw = 45f;
    public float swim_smooth = 10f;

    [Header("Jump Camera Parameters")]
    public float jump_sensitivity = 100f;
    public float jump_sensitivity_controller = 1000f;
    public float jump_minPitch = -45f;
    public float jump_maxPitch = 45f;
    public float jump_minYaw = -45f;
    public float jump_maxYaw = 45f;
    public float jump_smooth = 10f;

    [Header("Fall Camera Parameters")]
    public float fall_sensitivity = 100f;
    public float fall_sensitivity_controller = 1000f;
    public float fall_minPitch = -45f;
    public float fall_maxPitch = 45f;
    public float fall_minYaw = -45f;
    public float fall_maxYaw = 45f;
    public float fall_smooth = 10f;

    [Header("Air ride Camera Parameters")]
    public float air_sensitivity = 100f;
    public float air_sensitivity_controller = 1000f;
    public float air_minPitch = -45f;
    public float air_maxPitch = 45f;
    public float air_minYaw = -45f;
    public float air_maxYaw = 45f;
    public float air_smooth = 10f;
}
