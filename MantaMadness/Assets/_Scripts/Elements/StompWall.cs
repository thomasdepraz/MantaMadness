using UnityEngine;

public class StompWall : BreakableWall
{
    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if (controller.State == ControllerState.STOMP && isBroken == false)
            {
                isBroken = true;
                wall.SetActive(false);
                breakParticle.Play();
            }
        }
    }

}
