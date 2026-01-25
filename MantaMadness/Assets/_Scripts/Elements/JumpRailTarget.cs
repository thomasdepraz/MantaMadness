using UnityEngine;

public class JumpRailTarget : JumpTarget
{
    protected override void OnTriggerEnter(Collider other)
    {
        SimpleController controller = other.GetComponent<SimpleController>();
        if (controller != null)
        {
            NotifyPlayerHit(controller, other.ClosestPoint(transform.position));
            StartLaunchCoroutine();
        }
    }
}
