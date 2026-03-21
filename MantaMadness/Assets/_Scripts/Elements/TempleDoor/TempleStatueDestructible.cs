using UnityEngine;
using System.Collections;

public enum TempleStatueType
{
    Blue,
    Red,
    Green,
    Yellow,
}

public class TempleStatueDestructible : SpecialDestructible
{

    [SerializeField] public TempleStatueType type;

    public override void Start()
    {
        base.Start();
    }

    public override IEnumerator DestructionRoutine(Vector3 point)
    {
        yield return StartCoroutine(base.DestructionRoutine(point));

        if (manager is TempleDoorManager templeManager)
        {
            templeManager.OnDestructibleDestroyed(this);
        }
    }
}
