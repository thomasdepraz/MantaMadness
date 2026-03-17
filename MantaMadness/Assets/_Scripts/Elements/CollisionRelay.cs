using System;
using System.Collections.Generic;
using UnityEngine;

public class CollisionRelay : MonoBehaviour
{
    public Action<SimpleController> HitCollision;

    private HashSet<SimpleController> controllersInside = new HashSet<SimpleController>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            controllersInside.Add(controller);
            HitCollision?.Invoke(controller);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            // sécurité : ne pas appeler si déjà entré
            if (!controllersInside.Contains(controller))
            {
                controllersInside.Add(controller);
                HitCollision?.Invoke(controller);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            controllersInside.Remove(controller);
        }
    }
}
