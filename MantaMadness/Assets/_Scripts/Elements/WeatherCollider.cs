using UnityEngine;

public class WeatherCollider : MonoBehaviour
{
    [SerializeField] private CollisionRelay relay;

    [SerializeField] private WeatherType newWeather;

    private void Start()
    {
        relay.HitCollision += WeatherUpdate;
    }

    private void OnEnable()
    {
        relay.HitCollision += WeatherUpdate;
    }

    private void OnDisable()
    {
        relay.HitCollision -= WeatherUpdate;
    }

    private void WeatherUpdate(SimpleController overload)
    {
        WeatherManager.instance.SetNewWeather(newWeather);
    }
}
