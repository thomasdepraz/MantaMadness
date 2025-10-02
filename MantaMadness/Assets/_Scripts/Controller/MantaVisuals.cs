using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

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

    [Header("Particles")]
    public ParticleSystem surfParticles;
    public VisualEffect surfBladeEffect;
    public ParticleSystem splashParticles;
    public ParticleSystem styleParticles;
    public ParticleSystem[] driftParticles = new ParticleSystem[4];
    public ParticleSystem[] boostParticles = new ParticleSystem[3];
    public VisualEffect targetJumpParticles;
    public VisualEffect chargeJumpParticles;

    [Header("Visual")]
    public SkinnedMeshRenderer[] mantaBodyVisual;


    private int driftId = Animator.StringToHash("Drifting");
    private int driftDirId = Animator.StringToHash("DriftDirection");
    private int styleTriggerId = Animator.StringToHash("StyleTrigger");
    private int styleIndexId = Animator.StringToHash("Style");
    private int boostId = Animator.StringToHash("Boosting");
  

    private void Awake()
    {
        mantaController = GetComponent<SimpleController>();
        mantaController.stateChanged += UpdateState;
        mantaController.updateDrift += UpdateDrift;
        mantaController.boost += BoostParticles;
        mantaController.updateRaceTarget += SetArrowTarget;
        mantaController.dash += Dash;
        mantaController.triggerAnim += triggerAnimation;
        mantaController.enableBoolAnim += enableBoolAnimation;
        mantaController.disableBoolAnim+= disableBoolAnimation;
        mantaController.playTargetJumpParticles += JumpTargetParticles;
        mantaController.togglePlayerBodyVisual += ToggleMantaVisual;
    }

    private void OnDisable()
    {
        mantaController.stateChanged -= UpdateState;
        mantaController.updateDrift -= UpdateDrift;
        mantaController.boost -= BoostParticles;
        mantaController.updateRaceTarget -= SetArrowTarget;
        mantaController.dash -= Dash;
        mantaController.triggerAnim  -= triggerAnimation;
        mantaController.enableBoolAnim -= enableBoolAnimation;
        mantaController.disableBoolAnim -= disableBoolAnimation;
        mantaController.playTargetJumpParticles -= JumpTargetParticles;
        mantaController.togglePlayerBodyVisual -= ToggleMantaVisual;
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
        PlayerActionFMODManager.Instance.PlayStyleAction(PlayerActionFMOD.STYLE, dashCount);
    }

    private void Start()
    {
        arrow.gameObject.SetActive(false);
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.SURF);
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
        if(previous == ControllerState.FALLING && newState == ControllerState.SURFING || previous == ControllerState.JUMPING && newState == ControllerState.SURFING)
            SplashParticles();

        else if (previous == ControllerState.SURFING && newState == ControllerState.JUMPING)
            SplashParticles();

        //if (newState == ControllerState.JUMPING)
        //{
        //    if (mantaController.targetJumps == true)
        //    {
        //        mantaAnimator.SetTrigger("TargetJump");
        //    }
        //    else 
        //    {
        //        mantaAnimator.SetTrigger("Spin");
        //    }
        //}

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
    ExposedProperty surfBladePlayProperty = "State";
    ExposedProperty chargeJumpProperty = "State";
    private void UpdateParticles()
    {
        if (mantaController.State == ControllerState.SURFING && mantaController.HorizontalVelocity.magnitude > mantaController.controllerData.maxSpeed / 4f)
        {
            if ((!surfParticles.isPlaying))
                surfParticles.Play();

            if(mantaController.HorizontalVelocity.magnitude > mantaController.controllerData.maxSpeed + 5f)
            {
                //if (!surfBladeEffect.)
                if (surfBladeEffect.GetInt(surfBladePlayProperty) != 2)
                {
                    surfBladeEffect.SetInt(surfBladePlayProperty, 2);
                }
            }

            //if (!surfBladeEffect.)
            else if (surfBladeEffect.GetInt(surfBladePlayProperty) != 1)
            {
                surfBladeEffect.SetInt(surfBladePlayProperty, 1);
            }


        }
        else if (mantaController.State == ControllerState.SURFING && mantaController.HorizontalVelocity.magnitude <= mantaController.controllerData.maxSpeed / 4f || mantaController.State != ControllerState.SURFING)
        {
            surfParticles.Stop();
            if (surfBladeEffect.GetInt(surfBladePlayProperty) != 0)
            {
                surfBladeEffect.SetInt(surfBladePlayProperty, 0);
            }
        }

        if (mantaController.chargesJump == true && mantaController.State == ControllerState.SURFING)
        {
            if(mantaController.jumpChargeTimer >= mantaController.controllerData.jumpChargeTime)
            {
                chargeJumpParticles.SetInt(chargeJumpProperty, 2);
            }
            else
            {
                chargeJumpParticles.SetInt(chargeJumpProperty, 1);
            }
        }
        else if(mantaController.chargesJump == false || mantaController.State != ControllerState.SURFING)
        {
            chargeJumpParticles.SetInt(chargeJumpProperty, 0);
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

    public void triggerAnimation(string triggerName)
    {
        mantaAnimator.SetTrigger(triggerName);
    }

    ExposedProperty targetJumpDirection = "Direction";
    public void JumpTargetParticles()
    {
        targetJumpParticles.SetVector3(targetJumpDirection, transform.forward);
        targetJumpParticles.Play();
    }

    public void enableBoolAnimation(string boolName)
    {
        mantaAnimator.SetBool(boolName, true);
    }

    public void disableBoolAnimation(string boolName)
    {
        mantaAnimator.SetBool(boolName, false);
    }

    public void ToggleMantaVisual(bool toggle)
    {
        foreach (SkinnedMeshRenderer skin in mantaBodyVisual)
        {
            skin.enabled = toggle;
        }
    }
}
