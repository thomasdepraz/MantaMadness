using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections;

[RequireComponent(typeof(SplineAnimate))]
public class CarsSplineAnimate : MonoBehaviour
{
    private AudioSource HornAudio;
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
        splineAnimate = GetComponent<SplineAnimate>();
        HornAudio = GetComponent<AudioSource>();
        hitBox.HitCollision += CollisionCheck;
        audioHitBox.AudioCollision += PlayHorn;
    }

    void Start()
    {
        player = Game.Instance.player;
        splineAnimate.StartOffset = startPoint;
        splineAnimate.Play();
    }

    private void PlayHorn()
    {
        isAlive = true;
        HornAudio.Play();
    }

    private void CollisionCheck()
    {
        if(player.HorizontalVelocity.magnitude > player.controllerData.maxSpeed / 2)
        {
            StartCoroutine(KillSequence());
        }
        else
        {
            Game.Instance.Respawn(out Game.Instance.m_SpawnPosition, out Game.Instance.m_SpawnRotation);
        }
    }

    private void OnEnable()
    {
        splineAnimate.Play();
    }

    public IEnumerator KillSequence()
    {
        explosion.Play();
        visual.SetActive(false);
        isAlive = false;
        yield return new WaitForSeconds(10f);
        visual.SetActive(true);
        isAlive = true;
        yield return null;
    }
}
