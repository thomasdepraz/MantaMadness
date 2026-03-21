using System.Collections;
using UnityEngine;

public class SpecialDestructible : Destructible
{
    protected SpecialDestructibleManager manager;

    public void SetManager(SpecialDestructibleManager newManager)
    {
        manager = newManager;
    }

    public override IEnumerator DestructionRoutine(Vector3 point)
    {
        yield return StartCoroutine(base.DestructionRoutine(point));

        manager?.RegisterDestruction(this);
    }
}
