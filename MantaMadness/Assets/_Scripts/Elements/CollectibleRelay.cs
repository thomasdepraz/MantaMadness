using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class CollectibleRelay : MonoBehaviour
{
    public Action<GameObject> HitCollision;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            HitCollision?.Invoke(controller.gameObject);
        }
    }
}
