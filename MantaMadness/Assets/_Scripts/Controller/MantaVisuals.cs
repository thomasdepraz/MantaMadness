using System;
using System.Runtime.CompilerServices;
using UnityEditor.Animations;
using UnityEngine;

public class MantaVisuals : MonoBehaviour
{
    SimpleController mantaController;

    public Animator mantaAnimator;
    [Header("Animation parameters")]
    public int dashBlendTreeAnimationCount;

    [Header("Direction Arrow")]
    public Transform arrow;
    private Transform arrowTarget;

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
    public ParticleSystem styleParticles;
    public ParticleSystem[] driftParticles = new ParticleSystem[4];
    public ParticleSystem[] boostParticles = new ParticleSystem[3];

    private int driftId = Animator.StringToHash("Drifting");
    private int driftDirId = Animator.StringToHash("DriftDirection");
    private int styleTriggerId = Animator.StringToHash("StyleTrigger");
    private int styleIndexId = Animator.StringToHash("Style");
  

    private void Awake()
    {
        mantaController = GetComponent<SimpleController>();
        mantaController.stateChanged += UpdateState;
        mantaController.updateDrift += UpdateDrift;
        mantaController.boost += BoostParticles;
        mantaController.updateRaceTarget += SetArrowTarget;
        mantaController.dash += Dash;
    }

    private void Dash(int dashCount)
    {
        mantaAnimator.SetFloat(styleIndexId, UnityEngine.Random.Range(0, dashBlendTreeAnimationCount));
        mantaAnimator.SetTrigger(styleTriggerId);

        //PARTICLE EFFECT + SUN EFFECT DEPENDING ON DASHCOUNT
        if(dashCount > 4)
        {
            styleParticles.Play();
            UIEffectManager.Instance.GoodAction.Invoke();
             
        }

        var index = Mathf.Max(0, dashCount);
        PlayerActionFMODManager.Instance.PlayStyleAction(PlayerActionFMOD.STYLE, index);
    }

    private void Start()
    {
        arrow.gameObject.SetActive(false);
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
            SplashParticles();

        else if (previous == ControllerState.SURFING && newState == ControllerState.JUMPING)
            SplashParticles();

        if (newState == ControllerState.JUMPING)
        {
            mantaAnimator.SetTrigger("Spin");
        }

        if(newState == ControllerState.SWIMMING)
        {
            VolumeManager.Instance.toggleUnderwater(true);
            MusicManager.Instance.ToggleUnderwater();        
        }

        if(previous == ControllerState.SWIMMING)
        {
            VolumeManager.Instance.toggleUnderwater(false);
            MusicManager.Instance.ToggleUnderwater();
        }
    }

    private void Update()
    {
        UpdateModelRoll();
        UpdateParticles();

        mantaAnimator.SetBool(driftId, mantaController.IsDrifting);
        mantaAnimator.SetFloat(driftDirId, mantaController.DriftDirection);

        if(arrowTarget != null && arrow.gameObject.activeSelf)
        {
            Vector3 direction = (arrowTarget.position - arrow.position).normalized;
            arrow.forward = Vector3.Lerp(arrow.forward, new Vector3(direction.x, 0, direction.z), Time.deltaTime * 3);
        }
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
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.SPLASH);
    }

    private void BoostParticles()
    {
        // PLay sound
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.BOOST);
        for (int i = 0; i <  boostParticles.Length; i++)
        {
            boostParticles[i].Play();
        }
        UIParticleManager.Instance.playtSpecificParticle("SPEEDLINE", "");

        //play UI Sun Animation
        //UIManager.Instance.sunInterface.playGoodAnimation();
    }

    private void SetArrowTarget(Transform target)
    {
        arrowTarget = target;
        if (target == null)
            arrow.gameObject.SetActive(false);
        else
            arrow.gameObject.SetActive(true);
    }
}
