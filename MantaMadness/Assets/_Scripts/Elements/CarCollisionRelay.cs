using UnityEngine;
using System;
using System.Collections;

public class CarCollisionRelay : MonoBehaviour
{

    public Action<string> HitCollision;
    public Action AudioCollision;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            HitCollision?.Invoke("player");
        }
        else if (other.TryGetComponent(out GoldenCar car))
        {
            if (car.IsAlive)
                HitCollision?.Invoke("goldenCar");
        }
        else if (other.TryGetComponent(out CarsSplineAnimate car2))
        {
            if (car2.IsAlive)
            {
                if(car2.carType == CarType.Truck)
                {
                    HitCollision?.Invoke("truck");
                }
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            AudioCollision?.Invoke();
        }
    }
}
