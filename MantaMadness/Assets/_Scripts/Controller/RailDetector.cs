using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RailDetector : MonoBehaviour
{
    public SimpleController controller;
    private bool onRail;

    public void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Rail rail) && onRail is false)
        {
            if (controller.EnterRail(rail))
            {
                onRail = true;
            }
        }
    }

    Coroutine coroutine;
    public void ExitRail()
    {
        if(coroutine == null)
            coroutine = StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(0.2f);
        onRail = false;
        coroutine = null;
    }
}
