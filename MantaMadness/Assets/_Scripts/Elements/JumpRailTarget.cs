using DG.Tweening;
using System;
using UnityEngine;
using System.Collections;

public class JumpRailTarget : JumpTarget
{
    public enum RailEntryDirection
    {
        Forward,
        Backward
    }

    [Header("Rail Entry")]
    public RailEntryDirection entryDirection = RailEntryDirection.Forward;

    [Header("Visual")]
    [SerializeField] private GameObject visual;

    [Header("Particles")]
    [SerializeField] protected ParticleSystem burstParticle;

    public Rail.RailType dir;

    public event Action<Rail.RailType> railDirEvent;

    protected override void OnTriggerEnter(Collider other)
    {
        SimpleController controller = other.GetComponent<SimpleController>();

        if (controller != null && isAvailable)
        {
            StartLaunchCoroutine();
            NotifyPlayerHit(controller, other.ClosestPoint(transform.position));
        }
    }

    protected override void NotifyPlayerHit(SimpleController p, Vector3 contactPoint)
    {
        base.NotifyPlayerHit(p, contactPoint);
        railDirEvent?.Invoke(dir);
    }

    protected override IEnumerator LaunchCoroutine()
    {
        if (visual != null)
            visual.SetActive(false);

        if (burstParticle != null)
            burstParticle.Play();

        launchRoutine = null;
        yield break;
    }

    public override void DeactivateTarget()
    {
        if (!isAvailable) return;

        isAvailable = false;

        SetVisualState(JumpTargetVisualState.Inactive);
        currentState = JumpTargetVisualState.Inactive;

        var col = GetComponent<Collider>();
        if (CameraTargetDetection.Instance != null && col != null)
            CameraTargetDetection.Instance.validJumpTargets.Remove(col);

        ToggleFunctionElements(false);

        if (visual != null)
            visual.SetActive(false);
    }

    public void ReactivateTarget()
    {
        isAvailable = true;

        Collider col = GetComponent<Collider>();

        if (col != null)
            col.enabled = true;

        if (CameraTargetDetection.Instance != null && col != null)
            CameraTargetDetection.Instance.validJumpTargets.Add(col);

        if (visual != null)
        {
            visual.SetActive(true);

            visual.transform.localScale = Vector3.zero;
            visual.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        if (burstParticle != null)
            burstParticle.Play();

        if (indicator != null)
            indicator.gameObject.SetActive(true);

    }
}
