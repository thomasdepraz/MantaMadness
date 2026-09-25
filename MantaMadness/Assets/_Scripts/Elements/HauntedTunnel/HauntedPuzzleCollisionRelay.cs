using System;
using UnityEngine;
using UnityEngine.UI;

public class HauntedPuzzleCollisionRelay : MonoBehaviour
{

    public HauntedPuzzleElement parent;
    public Action<Vector3> HitCollision;

    private void OnTriggerEnter(Collider other)
    {
        //BREAKS ON SPIN CONTACT
        if(parent.type == HauntedType.Break)
        {
            if (other.TryGetComponent(out SpinBehavior spin))
            {
                if (spin.spinColEnabled || spin.spinBoostColEnabled)
                {
                    GetComponent<Collider>().enabled = false;
                    //BEHAVIOR
                    parent.CheckInteraction();
                }
            }
        }

        //BREAKS ON STOMP
        if(parent.type == HauntedType.Wall)
        {
            if (other.TryGetComponent(out SimpleController controller))
            {
                if (controller.State == ControllerState.STOMP)
                {
                    GetComponent<Collider>().enabled = false;
                    //BEHAVIOR
                    parent.CheckInteraction();
                }
            }
        }
    }


    private void OnTriggerStay(Collider other)
    {
        //BREAKS ON SPIN CONTACT
        if (parent.type == HauntedType.Break)
        {
            if (other.TryGetComponent(out SpinBehavior spin))
            {
                if (spin.spinColEnabled || spin.spinBoostColEnabled)
                {
                    if (GetComponent<Collider>().enabled == true)
                    {
                        GetComponent<Collider>().enabled = false;
                        parent.CheckInteraction();
                    }
                }
            }
        }
        //BREAKS ON STOMP
        if (parent.type == HauntedType.Wall)
        {
            if (other.TryGetComponent(out SimpleController controller))
            {
                if (controller.State == ControllerState.STOMP)
                {
                    GetComponent<Collider>().enabled = false;
                    //BEHAVIOR
                    parent.CheckInteraction();
                }
            }
        }

    }
}
