using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ControllerData", menuName = "Game Data/ControllerData")]
[Serializable]
public class ControllerData : ScriptableObject
{
    public float baseSpeed;
    public float baseSpeedModifier;
    public float baseTurnSpeed;
    public float brakeForce;
    [Range(0f,1f)]
    public float gripForce;
}
