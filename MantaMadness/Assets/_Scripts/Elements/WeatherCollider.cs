using UnityEngine;
using System.Collections;

public class WeatherCollider : MonoBehaviour
{
    [SerializeField] private CollisionRelay relay;

    [SerializeField] private WeatherType newWeather;

    [Header("Collectible Area")]
    [SerializeField]
    private string collectibleAreaID;

    private bool canTrigger;

    private IEnumerator Start()
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
        //WeatherManager.instance.SetNewWeather(newWeather);
        //WeatherManager.instance.SetCollectibleArea(collectibleAreaID);
        if (!canTrigger)
            return;

        Debug.Log(
    "WEATHER COLLIDER TRIGGERED : " +
    collectibleAreaID +
    " FRAME = " +
    Time.frameCount
);

        WeatherManager.instance.SetNewWeather(newWeather);

        if (!string.IsNullOrEmpty(collectibleAreaID))
        {
            CollectibleAreaRegistry.Instance.SetCurrentArea(
                collectibleAreaID
            );
        }
    }
}
