using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RailDetector : MonoBehaviour
{
    public SimpleController controller;
    private bool onRail;
    private bool onWaterfall;

    public void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Rail rail) && onRail is false)
        {
            if(rail.isRoadBorder == true && controller.strafRoutine != null || rail.isRoadBorder == true && controller.State != ControllerState.SURFING)
            {
                if (controller.EnterRail(rail))
                {
                    onRail = true;
                }
            }

            else if(rail.isRoadBorder == false)
            {
                if (controller.EnterRail(rail))
                {
                    onRail = true;
                }
            }

        }
        else if(other.TryGetComponent(out WaterFall waterfall) && onWaterfall is false)
        {
            if (controller.EnterWaterfall(waterfall))
            {
                onWaterfall = true;
            }
        }
    }

    Coroutine coroutine;
    public void ExitRail()
    {
        if(coroutine == null)
            coroutine = StartCoroutine(Cooldown());
    }

    public void ExitWaterfall()
    {
        if(coroutine == null)
            coroutine = StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(0.2f);
        onRail = false;
        onWaterfall = false;
        coroutine = null;
    }
}
