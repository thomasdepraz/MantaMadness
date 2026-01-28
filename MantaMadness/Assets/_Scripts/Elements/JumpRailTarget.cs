using System;
using UnityEngine;

public class JumpRailTarget : JumpTarget
{
    public Rail.RailType dir;

    public event Action<Rail.RailType> railDirEvent;

    protected override void OnTriggerEnter(Collider other)
    {
        SimpleController controller = other.GetComponent<SimpleController>();
        if (controller != null)
        {
            NotifyPlayerHit(controller, other.ClosestPoint(transform.position));
            StartLaunchCoroutine();
        }
    }

    protected override void NotifyPlayerHit(SimpleController p, Vector3 contactPoint)
    {
        base.NotifyPlayerHit(p, contactPoint);
        railDirEvent?.Invoke(dir);   
    }
}
