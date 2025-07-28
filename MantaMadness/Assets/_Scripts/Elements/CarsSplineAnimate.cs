using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections;

public class CarsSplineAnimate : MonoBehaviour
{
    [SerializeField] private AudioSource hornAudio;
    [SerializeField] private AudioSource explosionAudio;
    private SimpleController player;

    private SplineAnimate splineAnimate;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float startPoint;

    [SerializeField] private CarCollisionRelay hitBox;
    [SerializeField] private CarCollisionRelay audioHitBox;
    [SerializeField] private GameObject visual;

    [SerializeField] private ParticleSystem explosion;

    private bool isAlive = false;

    private void Awake()
    {
        if(GetComponent<SplineAnimate>() != null)
        {
            splineAnimate = GetComponent<SplineAnimate>();
        }
        hitBox.HitCollision += CollisionCheck;
        audioHitBox.AudioCollision += PlayHorn;
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
        if (splineAnimate != null)
        {
            hornAudio.Play();
        }
    }

    private void CollisionCheck(string type)
    {
        if(isAlive == true)
        {
            if(type == "player")
            {
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
        explosionAudio.Play();
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
