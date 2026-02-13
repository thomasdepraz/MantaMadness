using DG.Tweening;
using FMODUnity;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

public class MantaVisuals : MonoBehaviour
{
    public static MantaVisuals instance;

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
    public ParticleSystem[] boostParticles = new ParticleSystem[3];
    public VisualEffect targetJumpParticles;
    public ParticleSystem playerExplosionParticles;
    public VisualEffect reverseGrindParticles;
    public VisualEffect chargeJumpParticles;
    public VisualEffect chargeDriftParticles;
    public ParticleSystem chargeDriftParticlesAdditionnal;
    public VisualEffect railVisualEffect;
    public ParticleSystem railParticleSystem;
    public ParticleSystem pickupParticle;

    [Header("Visual")]
    public SkinnedMeshRenderer[] mantaAllVisuals;
    public SkinnedMeshRenderer stompVisual;
    public SkinnedMeshRenderer alienAntennaVisual;
    public SkinnedMeshRenderer doubleJumpGlassesVisual;
    public SkinnedMeshRenderer grindVisual;
    public SkinnedMeshRenderer[] catVisual;
    public Material[] playerMat;
    public GameObject playerMantaTrueBody;

    [Header("After Image")]
    public SkinnedMeshRenderer mantaBodyVisual;
    public Material afterImageMat;
    public float fadeDuration= 1f;
    public float interval = 0.05f;
    public float strafEffectDuration = 0.75f;


    private int driftId = Animator.StringToHash("Drifting");
    private int driftDirId = Animator.StringToHash("DriftDirection");
    private int styleTriggerId = Animator.StringToHash("StyleTrigger");
    private int styleIndexId = Animator.StringToHash("Style");
    private int boostId = Animator.StringToHash("Boosting");
    private int horizontalSpeedId = Animator.StringToHash("HorizontalSpeedFactor");
    private int verticalSpeedId = Animator.StringToHash("VerticalVelocity");


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        mantaController = GetComponent<SimpleController>();
        mantaController.updateDrift += UpdateDrift;
        mantaController.stateChanged += UpdateState;
        mantaController.boost += BoostParticles;
        mantaController.updateRaceTarget += SetArrowTarget;
        mantaController.dash += Dash;
        mantaController.triggerAnim += triggerAnimation;
        mantaController.enableBoolAnim += enableBoolAnimation;
        mantaController.disableBoolAnim+= disableBoolAnimation;
        mantaController.playTargetJumpParticles += JumpTargetParticles;
        mantaController.togglePlayerBodyVisual += ToggleMantaVisual;
        mantaController.straf += strafEffectsAndVisual;
        mantaController.afterImageEffect += AfterImageEffect;
        mantaController.updateEquipmentVisual += UpdateAbilityVisuals;
        mantaController.enterRail += StartGrindOnRail;
        //mantaController.railGrindAnim += GrindOnRail;
        mantaController.exitRail += ResetGrindOnRail;
        mantaController.enterWaterfall += StartWaterfall;
        mantaController.exitWaterfall += ExitWaterfall;
        mantaController.togglePlayerBlinkMat += ToggleBlink;
        mantaController.reverseGrinding += ReverseGrindOnRail;
        mantaController.stomplanding += StompLanding;
    }

    private void OnDisable()
    {
        mantaController.stateChanged -= UpdateState;
        mantaController.boost -= BoostParticles;
        mantaController.updateDrift -= UpdateDrift;
        mantaController.updateRaceTarget -= SetArrowTarget;
        mantaController.dash -= Dash;
        mantaController.triggerAnim  -= triggerAnimation;
        mantaController.enableBoolAnim -= enableBoolAnimation;
        mantaController.disableBoolAnim -= disableBoolAnimation;
        mantaController.playTargetJumpParticles -= JumpTargetParticles;
        mantaController.togglePlayerBodyVisual -= ToggleMantaVisual;
        mantaController.straf -= strafEffectsAndVisual;
        mantaController.afterImageEffect -= AfterImageEffect;
        mantaController.updateEquipmentVisual -= UpdateAbilityVisuals;
        mantaController.enterRail -= StartGrindOnRail;
        //mantaController.railGrindAnim -= GrindOnRail;
        mantaController.exitRail -= ResetGrindOnRail;
        mantaController.enterWaterfall -= StartWaterfall;
        mantaController.exitWaterfall -= ExitWaterfall;
        mantaController.togglePlayerBlinkMat -= ToggleBlink;
        mantaController.reverseGrinding -= ReverseGrindOnRail;
        mantaController.stomplanding -= StompLanding;
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

    private Coroutine afterImageRoutine;
    private IEnumerator SpawnAfterImageForDuration(float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            SpawnAfterImage();
            yield return new WaitForSeconds(interval);
            timer += interval;
        }
    }

    void SpawnAfterImage()
    {
        Mesh mesh = new Mesh();
        mantaBodyVisual.BakeMesh(mesh);

        Mesh snapshot = Instantiate(mesh);

        var ghost = AfterImagePool.Instance.GetGhost();
        ghost.Initialize(afterImageMat, fadeDuration);
        ghost.SetMesh(snapshot);
        ghost.Show(transform.position, Game.Instance.player.hoverBehaviour.normalContainer.transform.rotation, transform.localScale);
    }

    private void UpdateState(ControllerState previous, ControllerState newState)
    {
        if(previous == ControllerState.FALLING && newState == ControllerState.SURFING || 
            previous == ControllerState.JUMPING && newState == ControllerState.SURFING ||
            previous == ControllerState.STOMP && newState == ControllerState.SURFING)
            SplashParticles(newState);

        else if (previous == ControllerState.SURFING && newState == ControllerState.JUMPING)
            SplashParticles(newState);

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

        if(newState == ControllerState.FALLING)
        {
            mantaAnimator.SetBool("Falling", true);
        }

        if(previous == ControllerState.FALLING)
        {
            mantaAnimator.SetBool("Falling", false);
        }

        if (newState == ControllerState.SURFING)
        {
            mantaAnimator.SetBool("Surfing", true);
        }
        if (previous == ControllerState.SURFING)
        {
            mantaAnimator.SetBool("Surfing", false);
        }
    }

    private void Update()
    {
        UpdateModelRoll();
        UpdateParticles();
        UpdateAnimatorRatio(Game.Instance.player.HorizontalVelocity.magnitude, Game.Instance.player.controllerData.maxSpeed, horizontalSpeedId);
        UpdateAnimatorRatio(Mathf.Abs(Game.Instance.player.Velocity.y), Game.Instance.player.controllerData.gravity * Game.Instance.player.controllerData.limitFallingSpeedFactor, verticalSpeedId);

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
            //targetRotation = Quaternion.Euler(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z);
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
    ExposedProperty chargeDriftProperty = "State";
    ExposedProperty railVisualEffectProperty = "State";
    private void UpdateParticles()
    {
        if(mantaController.State == ControllerState.SURFING && mantaController.IsDrifting)
        {
            if(mantaController.DriftDirection == 1)
            {
                if (surfBladeEffect.GetInt(surfBladePlayProperty) != 4)
                {
                    surfBladeEffect.SetInt(surfBladePlayProperty, 4);
                }
            }
            else  if(mantaController.DriftDirection == -1)
            {
                if (surfBladeEffect.GetInt(surfBladePlayProperty) != 3)
                {
                    surfBladeEffect.SetInt(surfBladePlayProperty, 3);
                }
            }
            else
            {
                surfParticles.Stop();
                if (surfBladeEffect.GetInt(surfBladePlayProperty) != 0)
                {
                    surfBladeEffect.SetInt(surfBladePlayProperty, 0);
                }
            }
        }
        else if (mantaController.State == ControllerState.SURFING && mantaController.HorizontalVelocity.magnitude > mantaController.controllerData.maxSpeed / 4f)
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
        
        if (mantaController.State == ControllerState.RAIL)
        {
            if (railVisualEffect.GetInt(railVisualEffectProperty) != 1)
            {
                railVisualEffect.SetInt(railVisualEffectProperty, 1);
            }
            if(railParticleSystem.isPlaying == false)
            {
                railParticleSystem.Play();
            }
        }
        else
        {
            if (railVisualEffect.GetInt(railVisualEffectProperty) != 0)
            {
                railVisualEffect.SetInt(railVisualEffectProperty, 0);
            }
            if (railParticleSystem.isPlaying == true)
            {
                railParticleSystem.Stop();
            }
        }

    }

    private void UpdateDrift(bool drifting, bool boost, int xDir)
    {
        if (drifting)
        {
            if (boost)
            {
                chargeDriftParticles.SetInt(chargeDriftProperty, 2);
            }
            else
            {
                chargeDriftParticles.SetInt(chargeDriftProperty, 1);
            }
            chargeDriftParticlesAdditionnal.Play();
        }
        else
        {
            chargeDriftParticles.SetInt(chargeDriftProperty, 0);
            chargeDriftParticlesAdditionnal.Stop();
        }
    }
    private void SplashParticles(ControllerState newState)
    {
        splashParticles.Play();
        if(newState == ControllerState.SURFING)
        {
            PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.SPLASH);
        }

    }

    private void BoostParticles()
    {
        // PLay sound
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.BOOST);
        for (int i = 0; i <  boostParticles.Length; i++)
        {
            boostParticles[i].Play();
        }

        UIParticleManager.Instance.playtSpecificParticle(UiWordsParticles.SPEEDLINE, "");

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
    ExposedProperty ReverseGrindOffset = "Offset";
    ExposedProperty ReverseGrindLifeTime = "Lifetime";
    public void JumpTargetParticles()
    {
        targetJumpParticles.SetVector3(targetJumpDirection, transform.forward);
        targetJumpParticles.Play();
        playerExplosionParticles.Play();

        //PLAY SOUND
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.EXPLOSION);
    }

    public void PickupParticles()
    {
        pickupParticle.Play();
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
        foreach (SkinnedMeshRenderer skin in mantaAllVisuals)
        {
            skin.enabled = toggle;
        }
    }

    public void strafEffectsAndVisual()
    {
        AfterImageEffect(strafEffectDuration);
        triggerAnimation("Straf");     
    }

    bool railStrafRight = true;
    public void StartGrindOnRail()
    {
        railStrafRight = true;
    }

    public void ReverseGrindOnRail()
    {
        triggerAnimation("ReverseGrind");
        reverseGrindParticles.SetVector3(targetJumpDirection, new Vector3(0,0,90));
        reverseGrindParticles.SetVector3(ReverseGrindOffset, new Vector3(0,2,0));
        reverseGrindParticles.SetFloat(ReverseGrindLifeTime, mantaController.railReversePauseTime);
        reverseGrindParticles.Play();
        playerMantaTrueBody.transform.DOLocalRotate(new Vector3(0f,3240f,0f),mantaController.railReversePauseTime, RotateMode.FastBeyond360);
    }

    //public void GrindOnRail()
    //{
    //    AfterImageEffect(strafEffectDuration);
    //    if (railStrafRight == true)
    //    {
    //        foreach(ParticleSystem p in driftParticles)
    //        {
    //            if(p == driftParticles[1])
    //            {
    //                p.gameObject.SetActive(true);
    //                p.Play();
    //            }
    //            else
    //            {
    //                p.gameObject.SetActive(false);
    //            }
    //        }
    //        railStrafRight =false;
    //    }
    //    else 
    //    {
    //        foreach (ParticleSystem p in driftParticles)
    //        {
    //            if (p == driftParticles[0])
    //            {
    //                p.gameObject.SetActive(true);
    //                p.Play();
    //            }
    //            else
    //            {
    //                p.gameObject.SetActive(false);
    //            }
    //        }
    //        railStrafRight = true;
    //    }
    //        triggerAnimation("GrindSwitch");
    //}

    public void ResetGrindOnRail()
    {
        //foreach (ParticleSystem p in driftParticles)
        //{
        //    p.gameObject.SetActive(false);
        //}
    }

    public void AfterImageEffect(float duration)
    {
        if(afterImageRoutine != null)
        {
            afterImageRoutine = null;
            afterImageRoutine = StartCoroutine(SpawnAfterImageForDuration(duration));
        }
        else
        {
            afterImageRoutine = StartCoroutine(SpawnAfterImageForDuration(duration));
        }
    }

    public void UpdateAbilityVisuals()
    {
        //DOUBLE JUMP ABILITY VISUAL
        if(Game.Instance.player.doubleJumpAbility == true)
        {
            doubleJumpGlassesVisual.enabled = true;
        }
        else
        {
            doubleJumpGlassesVisual.enabled = false;
        }

        //STOMP ABILITY VISUAL
        if (Game.Instance.player.stompAbility == true)
        {
            stompVisual.enabled = true;
        }
        else
        {
            stompVisual.enabled = false;
        }

        //ALIEN ABILITY VISUAL
        if (Game.Instance.player.alienAntennasAbility == true)
        {
            alienAntennaVisual.enabled = true;
        }
        else
        {
            alienAntennaVisual.enabled = false;
        }

        //ALIEN ABILITY VISUAL
        if (Game.Instance.player.grindAbility== true)
        {
            grindVisual.enabled = true;
        }
        else
        {
            grindVisual.enabled = false;
        }

        //CAT ABILITY VISUAL
        if(Game.Instance.player.catAbility == true)
        {
            foreach(var visual in catVisual)
            {
                visual.enabled = true;
            }
        }
        else
        {
            foreach (var visual in catVisual)
            {
                visual.enabled = false;
            }
        }
    }

    public void UpdateAnimatorRatio(float value, float ratio, int id)
    {
        float newRatio = Mathf.Clamp01(value / ratio);
        mantaAnimator.SetFloat(id, newRatio);
    }

    private void StartWaterfall()
    {
        print("Enter waterfall");
    }

    private void ExitWaterfall()
    {
        print("Exit waterfall");
    }

    private void ToggleBlink(bool toggleValue, float speed = 0f)
    {
        if (toggleValue)
        {
            foreach(Material mat in playerMat)
            {
                mat.SetFloat("_BlinkEnabled", 1f);
                mat.SetFloat("_BlinkSpeed", speed);
            }
        }
        else
        {
            foreach (Material mat in playerMat)
            {
                mat.SetFloat("_BlinkEnabled", 0f);
            }
        }
    }

    private void StompLanding()
    {
        playerExplosionParticles.Play();

        //PLAY SOUND
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.EXPLOSION);
    }
}