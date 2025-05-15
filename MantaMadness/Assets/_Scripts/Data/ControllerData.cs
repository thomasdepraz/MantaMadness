using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ControllerData", menuName = "Game Data/ControllerData")]
[Serializable]
public class ControllerData : ScriptableObject
{
    [Header("Global parameters")]
    public float acceleration;
    public float maxSpeed;
    [Range(0f,1f)]
    public float overSpeedCoeff;
    public float baseTurnSpeed;
    public AnimationCurve speedToSteeringRatio;
    public float brakeForce;
    [Range(0f,1f)]
    public float grip;
    public float gravity;

    [Header("Surfing parameters")]
    public float hoverRaycastLength = 2f;
    public float hoverHeight = 0.7f;
    public float hoverStrength = 100f;
    public float hoverDamper = 5f;

    [Header("Diving parameters")]
    public float baseDivingDepth = 5f;
    public float maxDivingDepth = 20f;
    public AnimationCurve VelocityToDivingDepthRatio;
    public float baseDivingForce = 5f;
    public float underwaterDrag = 3f;
    public float jumpMultiplier = 1.2f;
    public float maxDivingFallingSpeed = 5;

    [Header("Jump parameters")]
    public float jumpDamping;
    public float coyoteTime;
    public float forwardImpulseForce;
    public float upwardImpulseForce;
    public float maxFallingSpeed;
    public float perfectLandingForce;

    [Header("Swimming parameters")]
    public float minimumFloatingForce;
    public float maximumFloatingForce;
    public float floatingForceMultiplier;

    [Header("Air Control")]
    public float maxAirControl;
    public float fallingAirControl;
    public float divingAirControl;

    [Header("Drift")]
    public float minSpeedToDrift;
    public float minSpeedToDriftBreak;
    public float driftTurnSpeed;
    [Min(1)]public float steeringMult;
    public float driftingGrip;
    public float driftBoostForce;
    public float driftBoostTimer;

    [Header("Air ride")]
    public float airRideVelocityThreshold;
    public float airRideGravityScale;
}
