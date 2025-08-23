using System;
using UnityEngine;

public class MoaiStatueCollisionRelay : MonoBehaviour
{
    public Action HitCollision; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if(controller.Velocity.magnitude > controller.controllerData.maxSpeed)
            {
                HitCollision?.Invoke();
            }
        }
    }
}
