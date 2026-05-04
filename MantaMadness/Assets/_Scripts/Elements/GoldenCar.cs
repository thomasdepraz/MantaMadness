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
}