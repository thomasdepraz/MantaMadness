using UnityEngine;

public class RailTarget : Rail
{
    private JumpTarget[] targets;

    protected override void Awake()
    {
        base.Awake();

        targets = GetComponentsInChildren<JumpTarget>(true);

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

    private void OnTargetHit(SimpleController player, Vector3 contactPoint)
    {
        if (!player.CanEnterRail())
            return;

        if (player.OnRail)
            return;

        player.EnterRail(this);
    }

}
