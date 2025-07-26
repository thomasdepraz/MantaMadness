using UnityEngine;
using System;
using System.Collections;

public class CarCollisionRelay : MonoBehaviour
{

    public Action HitCollision;
    public Action AudioCollision;

    private void OnTriggerEnter(Collider other)
    {
        HitCollision?.Invoke();
        AudioCollision?.Invoke();
    }
}
