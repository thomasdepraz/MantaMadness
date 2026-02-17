using System;
using UnityEngine;

public class DestructibleCollisionRelay : MonoBehaviour
{
    public Action<Vector3> HitCollision; 
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SpinBehavior spin))
        {
            if (spin.spinColEnabled || spin.spinBoostColEnabled)
            {
                HitCollision?.Invoke(other.ClosestPoint(transform.position));
                GetComponent<Collider>().enabled = false;
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out SpinBehavior spin))
        {
            if (spin.spinColEnabled || spin.spinBoostColEnabled)
            {
                if(GetComponent<Collider>().enabled == true)
                {
                    HitCollision?.Invoke(other.ClosestPoint(transform.position));
                    GetComponent<Collider>().enabled = false;
                }
            }
        }
    }
}
