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

    protected SimpleController player;

    protected SplineAnimate splineAnimate;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float startPoint;

    [SerializeField] private CarCollisionRelay hitBox;
    [SerializeField] private CarCollisionRelay audioHitBox;
    [SerializeField] private GameObject visual;

    [SerializeField] private ParticleSystem explosion;

    [SerializeField] public CarType carType = CarType.Car;

    [SerializeField] private EventReference hornAudio;
    [SerializeField] private EventReference explosionAudio;

    [SerializeField] protected bool isAlive = true;
    public bool IsAlive => isAlive;

    private Collider _hitCollider;
    private Collider _audioCollider;
    private bool _isDistanceCulled;
    private float _nextCullCheckTime;

    public float _pauseDistance;
    public float _resumeDistance;
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
        if (hitBox != null)
            _hitCollider = hitBox.GetComponent<Collider>();
        if (audioHitBox != null)
            _audioCollider = audioHitBox.GetComponent<Collider>();

        if(splineAnimate != null)
        {
            splineAnimate.StartOffset = startPoint;
            splineAnimate.Play();
        }
    }

    private void Update()
    {
        if (Time.time < _nextCullCheckTime)
            return;

        _nextCullCheckTime = Time.time + DistanceCullBand.CHECK_INTERVAL;

        if (isAlive == false || splineAnimate == null || player == null)
            return;

        float sqrDistance = (transform.position - player.transform.position).sqrMagnitude;
        float pauseDistanceSqr = _pauseDistance * _pauseDistance;
        float resumeDistanceSqr = _resumeDistance * _resumeDistance;

        if (_isDistanceCulled == false && sqrDistance > pauseDistanceSqr)
            ApplyDistanceCull();
        else if (_isDistanceCulled && sqrDistance < resumeDistanceSqr)
            ClearDistanceCull();
    }

    private void ApplyDistanceCull()
    {
        _isDistanceCulled = true;
        splineAnimate.Pause();
        splineAnimate.enabled = false;

        if (visual != null)
            visual.SetActive(false);

        if (_hitCollider != null)
            _hitCollider.enabled = false;
        if (_audioCollider != null)
            _audioCollider.enabled = false;
    }

    private void ClearDistanceCull()
    {
        _isDistanceCulled = false;

        if (visual != null)
            visual.SetActive(true);

        if (_hitCollider != null)
            _hitCollider.enabled = true;
        if (_audioCollider != null)
            _audioCollider.enabled = true;

        splineAnimate.enabled = true;
        splineAnimate.Play();
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

    protected virtual void CollisionCheck(string type)
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
                    if(player.IsLocked == false)
                    {
                        StartCoroutine(KillSequence());
                    }
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

            else if (type == "truck" && carType == CarType.Car)
            {
                StartCoroutine(FriendlyFire());
            }
        }
    }

    private void OnEnable()
    {
        _nextCullCheckTime = 0f;

        if (splineAnimate != null)
        {
            splineAnimate.Play();
        }
    }

    public virtual IEnumerator KillSequence()
    {
        explosion.Play();
        visual.SetActive(false);
        isAlive = false;

        OnCarDestroyed();

        RuntimeManager.PlayOneShot(explosionAudio, transform.position);
        UIEffectManager.Instance.ExplosionAction?.Invoke("Armature_TheRock");
        Game.Instance.player.boostBehaviour.IncrementGauge(BoostAction.CarCrash);

        yield return new WaitForSeconds(10f);

        visual.SetActive(true);
        isAlive = true;
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

    protected virtual void OnCarDestroyed()
    {
    }

}

public static class DistanceCullBand
{
    public const float PAUSE_DISTANCE = 150f;
    public const float RESUME_DISTANCE = 180f;
    public const float CHECK_INTERVAL = 0.25f;

    public static bool TryGetPlayerPosition(out Vector3 playerPosition)
    {
        playerPosition = Vector3.zero;

        if (Game.Instance == null || Game.Instance.player == null)
            return false;

        playerPosition = Game.Instance.player.transform.position;
        return true;
    }
}
