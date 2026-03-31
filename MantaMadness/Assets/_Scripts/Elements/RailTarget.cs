using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RailTarget : Rail
{
    private JumpRailTarget[] targets;

    private Coroutine reactivateRoutine;

    protected override void Awake()
    {
        base.Awake();

        targets = GetComponentsInChildren<JumpRailTarget>(true);

        foreach (var target in targets)
        {
            target.OnPlayerHit += OnTargetHit;
        }
    }

    private void OnDestroy()
    {
        if (targets == null) return;

        foreach (var target in targets)
        {
            target.OnPlayerHit -= OnTargetHit;
        }
    }

    private void OnTargetHit(JumpTarget target, SimpleController player, Vector3 contactPoint)
    {
        if (!player.CanEnterRail())
            return;

        if (player.OnRail)
            return;

        JumpRailTarget railTarget = target as JumpRailTarget;
        if (railTarget == null)
            return;

        Vector3 intentDir = GetTargetIntentDirection(railTarget, contactPoint);

        if (!player.EnterRail(this, intentDir))
            return;

        OnPlayerEnteredRail(player);
    }

    public void DisableTargets()
    {
        foreach (var t in targets)
        {
            t.DeactivateTarget();
        }
    }

    private IEnumerator ReactivateAfterRailExit(SimpleController player)
    {
        yield return new WaitUntil(() => player.CurrentRail != this);

        foreach (var t in targets)
        {
            StartCoroutine(RespawnTarget(t));
        }

        reactivateRoutine = null;
    }

    private IEnumerator RespawnTarget(JumpRailTarget target)
    {
        yield return new WaitForSeconds(target.respawnCooldown);

        target.ReactivateTarget();
    }

    public void OnPlayerEnteredRail(SimpleController player)
    {
        DisableTargets();

        if (reactivateRoutine != null)
            StopCoroutine(reactivateRoutine);

        reactivateRoutine = StartCoroutine(ReactivateAfterRailExit(player));
    }

    private Vector3 GetTargetIntentDirection(JumpRailTarget target, Vector3 contactPoint)
    {
        Vector3 localContact = transform.InverseTransformPoint(contactPoint);

        SplineUtility.GetNearestPoint(
            railSpline,
            localContact,
            out float3 nearest,
            out float t
        );

        railSpline.Evaluate(t, out _, out float3 localTangent, out _);

        Vector3 worldTangent =
            splineContainer.transform.TransformDirection(localTangent).normalized;

        if (target.entryDirection == JumpRailTarget.RailEntryDirection.Backward)
            worldTangent = -worldTangent;

        return worldTangent;
    }
}