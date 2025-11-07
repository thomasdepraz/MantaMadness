using System;
using UnityEngine;

public class CollisionRelay : MonoBehaviour
{
    public Action<SimpleController> HitCollision;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            HitCollision?.Invoke(controller);
        }
    }
}
