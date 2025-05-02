using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RailDetector : MonoBehaviour
{
    public SimpleController controller;

    public void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Rail rail))
        {
            controller.EnterRail(rail);
        }
    }
}
