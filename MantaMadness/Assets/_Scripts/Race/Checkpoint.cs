using System;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Action<Checkpoint> checkpointPassed;
    private bool canBePassed = false;
    private int raceIndex;
    public int RaceIndex { get => raceIndex; }
    public Transform respawnTransform;

    public void Activate(int raceIndex)
    {
        this.raceIndex = raceIndex;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }

    public void Reset()
    {
        canBePassed = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent(out SimpleController controller))
        {
            if(Vector3.Dot(transform.forward, controller.Velocity) > 0 && canBePassed)
            {
                checkpointPassed.Invoke(this);
                canBePassed = false;
            }
        }
    }
}
