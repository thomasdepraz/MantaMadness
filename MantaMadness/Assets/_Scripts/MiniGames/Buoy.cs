using System;
using UnityEngine;

public class Buoy : MonoBehaviour
{
    public new Collider collider;
    private BuoyGame game;
    public BuoyVisuals visuals;
    public Action onCollect;
    public Action onReset;

    public void Initialize(BuoyGame game)
    {
        this.game = game;
        collider.enabled = !game.Completed;

        visuals.SetCompleted(game.Completed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out SimpleController controller))
        {
            collider.enabled = false;
            game.Collect(this);
            onCollect.Invoke();
            SoundManager.PlayOneShotSound(SoundType.BUOYPASS);
        }
    }

    public void Reset()
    {
        collider.enabled = true;
        onReset?.Invoke();
    }
}
