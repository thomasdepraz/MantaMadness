using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public enum JumpTargetVisualState
{
    OutOfRange,
    Approaching,
    InRange,
    Inactive
}

public class JumpTarget : MonoBehaviour
{
    protected SimpleController player;
    [Header("Collision Layer")]
    [SerializeField] LayerMask playerMask;
    [Header("Particles")]
    [SerializeField] protected ParticleSystem indicator;
    [Header("Parameters")]
    [SerializeField] protected float respawnCooldown = 1f;

    public virtual event Action<SimpleController, Vector3> OnPlayerHit;

    [Header("Advanced Indicators (Optional)")]
    [SerializeField] private ParticleSystem greyIndicator;
    [SerializeField] private ParticleSystem approachingIndicator;
    [SerializeField] private ParticleSystem readyIndicator;

    [SerializeField] private Transform approachingTransform;
    [SerializeField] private Vector3 approachingScaleFar = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 approachingScaleNear = Vector3.one;
    [SerializeField] private float approachSmooth = 10f;

    private JumpTargetVisualState currentState = (JumpTargetVisualState)(-1);
    private float approachT;

    protected virtual void NotifyPlayerHit(SimpleController p, Vector3 contactPoint)
    {
        OnPlayerHit?.Invoke(p, contactPoint);
    }


    protected virtual void Start()
    {
        player = Game.Instance.player;
    }
    
    public void SwitchIndicatorVisibility(bool validTarget)
    {
        if (validTarget)
            SetVisualState(JumpTargetVisualState.InRange, 1f);
        else
            SetVisualState(JumpTargetVisualState.OutOfRange, 0f);
    }
    public bool isAvailable = true;

    public void DeactivateTarget()
    {
        if (!isAvailable) return;

        isAvailable = false;

        SetVisualState(JumpTargetVisualState.Inactive);
        currentState = JumpTargetVisualState.Inactive;

        var col = GetComponent<Collider>();
        if (CameraTargetDetection.Instance != null && col != null)
            CameraTargetDetection.Instance.validJumpTargets.Remove(col);

        ToggleFunctionElements(false);

        StartCoroutine(DisableCoroutine());
    }

    protected IEnumerator DisableCoroutine()
    {
        //if (CameraTargetDetection.Instance.validJumpTargets.Contains(gameObject.GetComponent<Collider>()))
        //    {
        //        CameraTargetDetection.Instance.validJumpTargets.Remove(gameObject.GetComponent<Collider>());
        //        print(gameObject.GetComponent<Collider>() + "Has been removed");
        //    }
        //ToggleFunctionElements(false);

        yield return new WaitForSeconds(respawnCooldown);
        SetVisualState(JumpTargetVisualState.OutOfRange);
        ToggleFunctionElements(true);
    }

    protected virtual void ToggleFunctionElements(bool toggleValue)
    {
        if (toggleValue)
        {
            //SET ANIMATION TO IDLE
            SetVisualState(JumpTargetVisualState.OutOfRange);
            gameObject.GetComponent<Collider>().enabled = true;
            isAvailable = true;
        }
        else if (!toggleValue)
        {
            //SET ANIMATION TO DISABLE
            gameObject.GetComponent<Collider>().enabled = false;
            isAvailable = false;
        }
    }

    public void StartLaunchCoroutine()
    {
        if (launchRoutine != null || !isAvailable) return;
        launchRoutine = StartCoroutine(LaunchCoroutine());
    }

    protected bool OnAnimationEvent = false;

    public Coroutine launchRoutine;
    protected virtual IEnumerator LaunchCoroutine()
    {
        yield return null;
        launchRoutine = null;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<SimpleController>() != null)
        {
            //DeactivateTarget();
        }
    }

    public void SetVisualState(JumpTargetVisualState state, float proximity01 = 0f)
    {
        proximity01 = Mathf.Clamp01(proximity01);

        if (currentState != state)
        {
            StopAllIndicators();

            currentState = state;

            switch (state)
            {
                case JumpTargetVisualState.Inactive:
                    break;

                case JumpTargetVisualState.OutOfRange:
                    if (greyIndicator != null)
                        greyIndicator.Play();
                    break;

                case JumpTargetVisualState.Approaching:
                    if (approachingIndicator != null)
                        approachingIndicator.Play();
                    break;

                case JumpTargetVisualState.InRange:
                    if (readyIndicator != null)
                        readyIndicator.Play();
                    break;

            }
        }

        if (state == JumpTargetVisualState.Approaching && approachingTransform != null)
        {
            approachT = Mathf.Lerp(
                approachT,
                proximity01,
                1f - Mathf.Exp(-approachSmooth * Time.deltaTime));

            approachingTransform.localScale =
                Vector3.Lerp(approachingScaleFar, approachingScaleNear, approachT);
        }
    }

    private void StopAllIndicators()
    {
        if (greyIndicator != null)
            greyIndicator.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (approachingIndicator != null)
            approachingIndicator.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (readyIndicator != null)
            readyIndicator.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
