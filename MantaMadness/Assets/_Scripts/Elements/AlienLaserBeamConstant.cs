using DG.Tweening;
using UnityEngine;

public class AlienLaserBeamConstant : AlienLaserBeam
{
    protected override void Start()
    {

    }

    protected override void OnEnable()
    {
    
    }

    protected override void OnDisable()
    {
        
    }

    protected override void CheckPlayerInLaser()
    {
        Collider[] hits = Physics.OverlapCapsule(startPoint.position, endPoint.position, radius, playerLayer);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out SimpleController controller))
            {
                Game.Instance.player.Kill(DeathType.ELECTROCUTED);
            }
        }
    }
}
