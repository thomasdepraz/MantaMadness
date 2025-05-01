using System;
using UnityEngine;

public class MantaVisuals : MonoBehaviour
{
    SimpleController mantaController;

    public Animator mantaAnimator;

    [Header("Rotation parameters")]
    public Transform modelTransform;
    public float rotationSpeed;

    [Header("Parameters")]
    public ParticleSystem surfParticles;
    public ParticleSystem splashParticles;

    public int driftId = Animator.StringToHash("Drifting");
    public int driftDirId = Animator.StringToHash("DriftDirection");
  

    private void Awake()
    {
        mantaController = GetComponent<SimpleController>();
        mantaController.stateChanged += UpdateState;
    }

    private void UpdateState(ControllerState previous, ControllerState newState)
    {
        if(previous == ControllerState.FALLING && newState == ControllerState.SURFING)
        {
            SplashParticles();
        }

        if(newState == ControllerState.JUMPING)
        {
            mantaAnimator.SetTrigger("Spin");
        }
    }

    private void Update()
    {
        UpdateModelRoll();
        UpdateParticles();

        mantaAnimator.SetBool(driftId, mantaController.IsDrifting);
        mantaAnimator.SetFloat(driftDirId, mantaController.DriftDirection);
    }
    Quaternion targetRotation;
    private void UpdateModelRoll()
    {
        targetRotation = Quaternion.identity;

        if(mantaController.State == ControllerState.SURFING || mantaController.State == ControllerState.SWIMMING)
        {
            float angular = mantaController.AngularVelocity.y;
            targetRotation = Quaternion.Euler(0, 0, -angular * 10);
        }

        //Falling
        if(mantaController.State == ControllerState.FALLING)
        {
            float magnitude = mantaController.HorizontalVelocity.magnitude;
            float pitch = Vector3.Dot(mantaController.HorizontalVelocity.normalized, transform.forward);
            float roll = Vector3.Dot(mantaController.HorizontalVelocity.normalized, transform.right);
            targetRotation = Quaternion.Euler(pitch * magnitude * 2, 0, -roll * magnitude * 2);
        }

        //Diving
        if(mantaController.State == ControllerState.DIVING)
        {
            float ratio = mantaController.Velocity.y / -10;
            float maxPitch = Mathf.Lerp(0, 80f, ratio);
            targetRotation = Quaternion.Euler(maxPitch, 0, 0);
        }

        if (mantaController.State == ControllerState.JUMPING || mantaController.State == ControllerState.SWIMMING)
        {
            float ratio = mantaController.Velocity.y / 10;
            float maxPitch = Mathf.Lerp(0, -80f, Mathf.Clamp01(ratio));
            targetRotation = Quaternion.Euler(maxPitch, 0, 0);
        }

        modelTransform.localRotation = Quaternion.Lerp(modelTransform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void UpdateParticles()
    {
        if(mantaController.State == ControllerState.SURFING && mantaController.HorizontalVelocity.magnitude > 0.5f)
        {
            if (!surfParticles.isPlaying)
                surfParticles.Play();
        }
        else
        {
            surfParticles.Stop();
        }
    }

    private void SplashParticles()
    {
        splashParticles.Play();
    }
}
