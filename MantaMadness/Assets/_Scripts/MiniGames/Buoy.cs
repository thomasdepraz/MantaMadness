using System;
using UnityEngine;

public class Buoy : MonoBehaviour
{
    public Collider collider;
    private BuoyGame game;
    public Action onCollect;
    public Action onReset;

    public void Initialize(BuoyGame game)
    {
        this.game = game;
        collider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out SimpleController controller))
        {
            collider.enabled = false;
            game.Collect(this);
            onCollect.Invoke();
        }
    }

    public void Reset()
    {
        collider.enabled = true;
        onReset.Invoke();
    }
}
