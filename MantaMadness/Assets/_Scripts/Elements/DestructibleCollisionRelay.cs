using System;
using UnityEngine;

public class DestructibleCollisionRelay : MonoBehaviour
{
    public Action<float,Vector3> HitCollision; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
                HitCollision?.Invoke(controller.Velocity.magnitude,other.ClosestPoint(transform.position));
        }
    }
}
