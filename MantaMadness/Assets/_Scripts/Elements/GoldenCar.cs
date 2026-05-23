using FMODUnity;
using UnityEngine;
using UnityEngine.Splines;
using System.Collections;

public class GoldenCar : CarsSplineAnimate
{
    [SerializeField] private string coinName;

    protected override void OnCarDestroyed()
    {
        CoinManager.Instance.ActivateCoinHolder(coinName);
    }

    public override IEnumerator KillSequence()
    {
        yield return base.KillSequence();
    }

    protected override void CollisionCheck(string type)
    {
        if (isAlive == true)
        {
            if (type == "player")
            {
                switch (carType)
                {
                    case CarType.Truck:
                        player.Kill(DeathType.FLATTEN);
                        return;
                }

                if (player.HorizontalVelocity.magnitude > player.controllerData.maxSpeed / 2 || splineAnimate == null)
                {
                    StartCoroutine(KillSequence());
                }
                else
                {
                    if (splineAnimate != null)
                    {
                        Game.Instance.Respawn(out Game.Instance.m_SpawnPosition, out Game.Instance.m_SpawnRotation);
                    }
                }
            }
        }
    }
}