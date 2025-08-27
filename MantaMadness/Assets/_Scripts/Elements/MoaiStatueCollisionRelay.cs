using System;
using UnityEngine;

public class MoaiStatueCollisionRelay : MonoBehaviour
{
    public Action<float> HitCollision; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
                HitCollision?.Invoke(controller.Velocity.magnitude);
        }
    }
}
