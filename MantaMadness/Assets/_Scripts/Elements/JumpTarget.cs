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
    [SerializeField] public float respawnCooldown = 1f;

    public virtual event Action<JumpTarget, SimpleController, Vector3> OnPlayerHit;

    [Header("Advanced Indicators (Optional)")]
    [SerializeField] private ParticleSystem greyIndicator;
    [SerializeField] private ParticleSystem approachingIndicator;
    [SerializeField] private ParticleSystem readyIndicator;

    [SerializeField] private Transform approachingTransform;
    [SerializeField] private float approachSmooth = 10f;

    protected JumpTargetVisualState currentState = (JumpTargetVisualState)(-1);

    protected virtual void NotifyPlayerHit(SimpleController p, Vector3 contactPoint)
    {
        OnPlayerHit?.Invoke(this, p, contactPoint);
    }


    protected virtual void Start()
    {
        player = Game.Instance.player;
    }
    
    public void SwitchIndicatorVisibility(bool validTarget)
    {

    }
    public bool isAvailable = true;

    public virtual void DeactivateTarget()
    {
        if (!isAvailable) return;

        isAvailable = false;

        currentState = JumpTargetVisualState.Inactive;

        var col = GetComponent<Collider>();
        if (CameraTargetDetection.Instance != null && col != null)
        {
            CameraTargetDetection.Instance.NotifyJumpTargetPopped(col);
            CameraTargetDetection.Instance.validJumpTargets.Remove(col);
        }

        ToggleFunctionElements(false);

        StartCoroutine(DisableCoroutine());
    }

    protected IEnumerator DisableCoroutine()
    {
        yield return new WaitForSeconds(respawnCooldown);
        ToggleFunctionElements(true);
    }

    protected virtual void ToggleFunctionElements(bool toggleValue)
    {
        if (toggleValue)
        {
            //SET ANIMATION TO IDLE
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

    public void SetAsCurrentTarget(bool active)
    {
        if (!isAvailable)
        {
            StopAllIndicators();
            return;
        }

        if (active)
        {
            if (readyIndicator != null && !readyIndicator.isPlaying)
                readyIndicator.Play();
        }
        else
        {
            if (readyIndicator != null)
                readyIndicator.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
