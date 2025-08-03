using UnityEngine;
using UnityEngine.Splines;
using System;
using System.Collections;

public class GoldenCar : MonoBehaviour
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

    public bool isAlive = false;

    [SerializeField] private Rigidbody rb;
    [SerializeField] private string coinName;

    private void Awake()
    {
        splineAnimate = GetComponent<SplineAnimate>();
        hitBox.HitCollision += CollisionCheck;
        audioHitBox.AudioCollision += PlayHorn;
    }

    void Start()
    {
        player = Game.Instance.player;
        isAlive = true;
        splineAnimate.StartOffset = startPoint;
        splineAnimate.Play();
    }

    private void PlayHorn()
    {
        hornAudio.Play();
    }

    private void CollisionCheck(string type)
    {
        if (isAlive == true)
        {
            if (type == "player")
            {
                if (player.HorizontalVelocity.magnitude > player.controllerData.maxSpeed / 2)
                {
                    StartCoroutine(KillSequence());
                    CoinManager.Instance.ActivateCoinHolder(coinName);
                }
                else
                {
                    Game.Instance.Respawn(out Game.Instance.m_SpawnPosition, out Game.Instance.m_SpawnRotation);
                }
            }
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
        explosionAudio.Play();
        UIEffectManager.Instance.ExplosionAction?.Invoke("Armature_TheRock");
        Game.Instance.player.boostBehaviour.IncrementGauge(BoostAction.GoldenCarCrash);
        yield return new WaitForSeconds(10f);
        visual.SetActive(true);
        isAlive = true;
        yield return null;
    }
}
