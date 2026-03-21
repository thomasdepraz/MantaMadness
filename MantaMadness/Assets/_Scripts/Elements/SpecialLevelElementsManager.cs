using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class SpecialDestructibleManager : MonoBehaviour
{
    [SerializeField] protected List<SpecialDestructible> destructibles = new();

    protected int destroyedCount;

    [SerializeField] protected UnityEvent onAllDestroyed;

    protected virtual void Start()
    {
        foreach (var destructible in destructibles)
        {
            if (destructible == null)
            {
                Debug.LogWarning("Missing destructible in manager list", this);
                continue;
            }

            destructible.SetManager(this);
        }
    }

    public virtual void RegisterDestruction(SpecialDestructible destructible)
    {
        if (!destructibles.Contains(destructible))
            return;

        destroyedCount++;

        Debug.Log($"Destroyed {destroyedCount}/{destructibles.Count}");

        if (destroyedCount >= destructibles.Count)
        {
            ActivateEvent();
        }
    }

    protected virtual void ActivateEvent()
    {
        Debug.Log("Module completed!");
        onAllDestroyed?.Invoke();
    }
}