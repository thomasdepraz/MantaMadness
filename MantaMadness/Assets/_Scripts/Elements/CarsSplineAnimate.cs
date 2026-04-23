using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections;
using FMODUnity;

public enum CarType
{
    Car,
    TribouliBoat,
    Truck,
}

public class CarsSplineAnimate : MonoBehaviour
{

    private SimpleController player;

    private SplineAnimate splineAnimate;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float startPoint;

    [SerializeField] private CarCollisionRelay hitBox;
    [SerializeField] private CarCollisionRelay audioHitBox;
    [SerializeField] private GameObject visual;

    [SerializeField] private ParticleSystem explosion;

    [SerializeField] private CarType carType = CarType.Car;

    [SerializeField] private EventReference hornAudio;

    private bool isAlive = true;

    private void Awake()
    {
        if(GetComponent<SplineAnimate>() != null)
        {
            splineAnimate = GetComponent<SplineAnimate>();
        }
        hitBox.HitCollision += CollisionCheck;
        if(audioHitBox != null)
        {
            audioHitBox.AudioCollision += PlayHorn;
        }
    }

    void Start()
    {
        player = Game.Instance.player;
        isAlive = true;
        if(splineAnimate != null)
        {
            splineAnimate.StartOffset = startPoint;
            splineAnimate.Play();
        }
    }

    private void PlayHorn()
    {
        switch (carType)
        {
            case CarType.Car:
                //Check if car is a moving car
                if (splineAnimate != null)
                {
                    RuntimeManager.PlayOneShot(hornAudio, transform.position);
                }
                break;
            case CarType.TribouliBoat:
                break;
            case CarType.Truck:
                break;
        }
    }

    private void CollisionCheck(string type)
    {
        if(isAlive == true)
        {
            if(type == "player")
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
                    if(splineAnimate != null)
                    {
                        Game.Instance.Respawn(out Game.Instance.m_SpawnPosition, out Game.Instance.m_SpawnRotation);
                    }
                }
            }
            else if (type == "goldenCar")
            {
                StartCoroutine(FriendlyFire());
            }
        }
    }

    private void OnEnable()
    {
        if (splineAnimate != null)
        {
            splineAnimate.Play();
        }
    }

    public IEnumerator KillSequence()
    {
        explosion.Play();
        visual.SetActive(false);
        isAlive = false;
        UIEffectManager.Instance.ExplosionAction?.Invoke("Armature_TheRock"); 
        Game.Instance.player.boostBehaviour.IncrementGauge(BoostAction.CarCrash);
        yield return new WaitForSeconds(10f);
        visual.SetActive(true);
        isAlive = true;
        yield return null;
    }

    public IEnumerator FriendlyFire()
    {
        explosion.Play();
        visual.SetActive(false);
        isAlive = false;
        yield return new WaitForSeconds(5f);
        visual.SetActive(true);
        isAlive = true;
        yield return null;
    }
}
