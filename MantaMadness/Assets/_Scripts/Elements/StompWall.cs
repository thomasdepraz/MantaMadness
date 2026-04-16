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
            if(isBroken == false)
            {
                if (controller.State == ControllerState.STOMP || controller.State == ControllerState.ANTIGRAVJUMP)
                {
                    isBroken = true;
                    wall.SetActive(false);
                    breakParticle.Play();
                }
            }

        }
    }

}
