using UnityEngine;
using System.Collections;

public class WeatherCollider : WorldCheckpoint
{
    [SerializeField] private CollisionRelay relay;

    [SerializeField] private WeatherType newWeather;

    private bool canTrigger;

    private SimpleController player;

    protected override void Start()
    {
        base.Start();

        if (player == null)
        {
            player = Game.Instance.player;
        }

        StartCoroutine(DelayStart());
    }

    private IEnumerator DelayStart()
    {
        yield return null;
        yield return null;

        canTrigger = true;
        relay.HitCollision += WeatherUpdate;
    }

    private void OnDisable()
    {
        relay.HitCollision -= WeatherUpdate;
    }

    private void WeatherUpdate(SimpleController overload)
    {
        if (!canTrigger)
            return;

        WeatherManager.instance.SetNewWeather(newWeather);

        CollectibleAreaRegistry.Instance.SetCurrentArea(collectibleAreaID);

    }

    public override void EnableMat()
    {
        //RIEN
    }

    public override void DisableMat()
    {
        //RIEN
    }
}
