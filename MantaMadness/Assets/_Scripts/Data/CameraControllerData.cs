using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraControllerData", menuName = "Game Data/CameraControllerData")]
[Serializable]
public class CameraControllerData : ScriptableObject
{
    [Header("General Camera Parameters")]
    public float sensitivity = 100f;
    public float sensitivity_controller = 1000f;
    public float minPitch = -45f;
    public float maxPitch = 45f;
    public float smooth = 10f;

    [Header("Stomp Camera Parameters")]
    public float stomp_minPitch = 90f;
    public float stomp_maxPitch = 95f;
}
