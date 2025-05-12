using System;
using UnityEngine;

public class MantaVisuals : MonoBehaviour
{
    SimpleController mantaController;

    public Animator mantaAnimator;

    [Header("Rotation parameters")]
    public Transform modelTransform;
    public float driftTurnAngle;
    public float surfTurnAngle;
    public float airControlAngle;
    public float maxVerticalSpeed;
    public float divingAngle;
    public float airRideAngle;
    public float rotationSpeed;

    [Header("Parameters")]
    public ParticleSystem surfParticles;
    public ParticleSystem splashParticles;
    public ParticleSystem[] driftParticles = new ParticleSystem[4];

    private int driftId = Animator.StringToHash("Drifting");
    private int driftDirId = Animator.StringToHash("DriftDirection");
  

    private void Awake()
    {
        mantaController = GetComponent<SimpleController>();
        mantaController.stateChanged += UpdateState;
        mantaController.updateDrift += UpdateDrift;
    }

    private void UpdateDrift(int driftDir, bool drifting, bool boost)
    {
        for (int i = 0; i < driftParticles.Length; i++)
        {
            driftParticles[i].Stop();
            driftParticles[i].gameObject.SetActive(false);
        }

        if(drifting)
        {
            if(driftDir > 0)
            {
                int index = boost ? 3 : 2;
                driftParticles[index].gameObject.SetActive(true);
                driftParticles[index].Play();
            }
            else
            {
                int index = boost ? 1 : 0;
                driftParticles[index].gameObject.SetActive(true);
                driftParticles[index].Play();
            }
        }
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
            targetRotation = Quaternion.Euler(0, 0, -angular * (mantaController.IsDrifting ? driftTurnAngle : surfTurnAngle));
        }

        //Falling
        if(mantaController.State == ControllerState.FALLING)
        {
            Vector3 dir = new Vector3(mantaController.AirControlDirection.x, 0, mantaController.AirControlDirection.y);
            float magnitude = Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.z));
            targetRotation = Quaternion.Euler(dir.z * magnitude * airControlAngle, 0, -dir.x * magnitude * airControlAngle);
        }

        //Diving
        if(mantaController.State == ControllerState.DIVING)
        {
            float ratio = mantaController.Velocity.y / -maxVerticalSpeed;
            float maxPitch = Mathf.Lerp(0, divingAngle, Mathf.Clamp01(ratio));
            targetRotation = Quaternion.Euler(maxPitch, 0, 0);
        }

        if (mantaController.State == ControllerState.JUMPING || mantaController.State == ControllerState.SWIMMING || mantaController.State == ControllerState.AIRRIDE)
        {
            float ratio = mantaController.Velocity.y / maxVerticalSpeed;
            float maxPitch = Mathf.Lerp(0, -airRideAngle, Mathf.Clamp01(ratio));
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
