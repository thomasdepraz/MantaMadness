using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ControllerData", menuName = "Game Data/ControllerData")]
[Serializable]
public class ControllerData : ScriptableObject
{
    [Header("Global parameters")]
    public float baseSpeed;
    public float baseSpeedModifier;
    public float baseTurnSpeed;
    public float brakeForce;
    [Range(0f,1f)]
    public float gripForce;

    [Header("Surfing parameters")]
    public float hoverRaycastLength = 2f;
    public float hoverHeight = 0.7f;
    public float hoverStrength = 100f;
    public float hoverDamper = 5f;

    [Header("Diving parameters")]
    public float baseDivingDepth = 5f;
    public float baseDivingForce = 5f;
    public float underwaterDrag = 4f;
    public float jumpMultiplier = 1.2f;

    [Header("Jump parameters")]
    public float forwardImpulseForce;
    public float upwardImpulseForce;

}
