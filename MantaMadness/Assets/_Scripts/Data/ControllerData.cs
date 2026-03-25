using System;
using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(fileName = "ControllerData", menuName = "Game Data/ControllerData")]
[Serializable]
public class ControllerData : ScriptableObject
{
    [Header("Global parameters")]
    public float acceleration;
    public float maxSpeed;
    public float rotationTorque;
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
    public float hoverAlignementSpeed = 2f;
    public float maxAngleToHover = 100f;

    [Header("Style parameters")]
    public float minSpeedToDash;
    public float dashTimer;
    public float dashForce;
    public float styleCooldown;
    public float dashTimeThreshold;
    public int maxConsecutiveDashCount;
    public bool canDriftandDash;
    public float styleTriggerRadius;

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
    public float jumpForceMultiplierMin;
    public float jumpForceMultiplierMax;
    public float jumpChargeTime;

    [Header("Swimming parameters")]
    public float minimumFloatingForce;
    public float maximumFloatingForce;
    public float floatingForceMultiplier;

    [Header("Air Control")]
    public float maxAirControl; 
    public float airControlDrag;
    public float airControlRotationSpeed;
    public float fallingAirControl;
    public float divingAirControl;
    public float maxAirTime;
    public float maxAirTimeGravityFactor;
    public float limitFallingSpeedFactor;

    [Header("Drift")]
    public float minSpeedToDrift;
    public float driftTurnSpeed;
    public float driftMoveSpeed;
    [Range(0,1)]public float steeringRemapMax;
    [Range(0,1)]public float steeringRemapMin;
    public float lateralSpeed;
    public float forwardDriftDrag;
    public float driftDrag;
    public float driftBoostTimer;
    public float driftDirectionalDecay;
    public float driftBrakeDecay;

    [Header("Boost")]
    public float boostForce;
    public float superBoostForce;
    public float superBoostAfterImageEffectDuration;
    public float boostAfterImageEffectDuration;
    public float boostCooldown;

    [Header("Straf")]
    public float strafForce;
    public float strafForwardForce;
    public float strafCooldown;

    [Header("Air ride")]
    public float airRideVelocityThreshold;
    public float airRideGravityScale;


    [Header("Target Jump")]
    public float targetRaycastLength = 2f;
    public float targetRaycastRadius = 5f;
    public float targetJumpSpeed;
    public LayerMask targetObjectsMask;
    public float targetDetectionRadius;
    public float targetBoostFactor;
    public float targetBounceForce;

    [Header("Double Jump")]
    public float doubleJumpUpForce;
    public float doubleJumpForwardForce;
    public float doubleJumpDamping;

    [Header("Stomp")]
    public float stompForce;
    public float stompUpForce;
    public float stompChargeTime;
    public float stompAfterImageEffectTime;
    public float stompAccelForce;
    public float stompJumpCancelUpForce;
    public float stompJumpCancelForwardForce;
    public float stompCancelRange = 2.5f;
    public float stompActionWindowTime = 0.35f;
    public float stompJumpBonusUpForceMult = 1.2f;
    public float stompJumpBonusForwardForceMult = 0.5f;
    public float stompHitStopDuration = 0.1f;
    public float stompActionBuildupWindowTime = 0.5f;
    public float stompSpinCancelUpForce;

    [Header("NPC Interaction")]
    public float npcInteractionRadius;

    [Header("Rail")]
    public float railImpulseForce;
    public float railExitForce;
    public float railTransferForce = 20f;

    [Header("Bump Parameters")]
    public float bumpDetectionRadius;
    public float bumpRaycastLenght;
    public float bumpForce;

    [Header("Spin Paramaters")]
    public float spinBoostTimer = 1f;
    public float spinBounceTimer = 0.75f;
    public float spinForce = 50f;
    public float spinPerfectBonusForce = 70f;

    [Header("Electric Jump")]
    public float electricJumpSpeed = 40f;
    public float electricJumpDuration = 0.35f;
    public float electricJumpExitVelocityFactor = 0.5f;

}
