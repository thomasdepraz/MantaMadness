using DG.Tweening;
using FMODUnity;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class PoulpsBehavior : MonoBehaviour
{
    [Header("Spline Components")]
    [SerializeField] private SplineAnimate splinePlayer;
    [SerializeField] private SplineContainer path;

    [Header("Particles")]
    [SerializeField] private ParticleSystem sleepParticles;
    [SerializeField] private ParticleSystem exclamationParticles;
    [SerializeField] private ParticleSystem smokeInkParticles;


    [Header("Components")]
    [SerializeField] private Transform body;
    [SerializeField] private Collectible[] collectible;
    [SerializeField] private Vector3 offset;

    [Header ("Poulp Visual + Anim")]
    [SerializeField] private GameObject visual;
    [SerializeField] private Animator animator;
    [SerializeField] private PoulpsRelay relay;


    [Header("Sound")]
    [SerializeField] private EventReference poulpStartled;
    [SerializeField] private EventReference poulpMoveLoopReference;
    public FMOD.Studio.EventInstance poulpMoveLoopEvent;

    [Header("Ground Alignment")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float raycastHeight = 2f;
    [SerializeField] private float raycastDistance = 5f;
    [SerializeField] private float alignSpeed = 10f;
    [SerializeField] private Transform visualToRotate;

    private bool hasActivated =  false;

    private void Start()
    {
        if (collectible.Length > 0) 
        {
            foreach (var item in collectible)
            {
                item.gameObject.SetActive(false);
            }
        }
        sleepParticles.Play();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if(hasActivated == false)
            {
                hasActivated = true;
                StartCoroutine(Suprised());
            }
        }
    }

    private void OnEnable()
    {
        relay.AnimationTriggerAction += AnimationEventTrigger;
    }

    private void OnDisable()
    {
        relay.AnimationTriggerAction -= AnimationEventTrigger;
    }

    private IEnumerator spawnCoroutine()
    {
        splinePlayer.Play();

        for (int i = 0; i < collectible.Length; i++)
        {
            if (i == 0)
            {
                if (collectible[i].State == CollectibleState.Activable)
                {
                    collectible[i].ActivateCollectible();
                    collectible[i].gameObject.transform.position = body.transform.position + offset;
                    collectible[i].transform.DOMoveY(collectible[i].transform.position.y + 5f, 0.2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
                }
                yield return new WaitForSeconds(splinePlayer.Duration / (collectible.Length + 3));
            }
            else
            {
                if (collectible[i].State == CollectibleState.Activable)
                {
                    collectible[i].ActivateCollectible();
                    collectible[i].gameObject.transform.position = body.transform.position + offset;
                    collectible[i].transform.DOMoveY(collectible[i].transform.position.y + 5f,0.2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
                }
                yield return new WaitForSeconds(splinePlayer.Duration / (collectible.Length + 3));
            }
        }

    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(splinePlayer.Duration);
        splinePlayer.Pause();
        visual.SetActive(false);
        smokeInkParticles.Stop();
        smokeInkParticles.gameObject.SetActive(false);
        poulpMoveLoopEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        poulpMoveLoopEvent.release();
    }

    private bool OnAnimationEvent = false;
    private IEnumerator Suprised()
    {
        sleepParticles.Stop();
        sleepParticles.gameObject.SetActive(false);
        exclamationParticles.Play();
        animator.SetTrigger("Suprised");
        RuntimeManager.PlayOneShot(poulpStartled, transform.position);
        yield return new WaitUntil(() => OnAnimationEvent);
        animator.SetTrigger("Sprint");
        poulpMoveLoopEvent = RuntimeManager.CreateInstance(poulpMoveLoopReference);
        poulpMoveLoopEvent.start();
        smokeInkParticles.Play();
        OnAnimationEvent = false;
        StartCoroutine(spawnCoroutine());
        StartCoroutine(Timer());
    }

    private void FixedUpdate()
    {
        if (poulpMoveLoopEvent.isValid())
        {
            poulpMoveLoopEvent.set3DAttributes(RuntimeUtils.To3DAttributes(transform));
        }

        AlignToGround();
    }

    private void AnimationEventTrigger()
    {
        OnAnimationEvent = true;
    }

    private void AlignToGround()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            Transform target = visualToRotate != null ? visualToRotate : transform;

            Vector3 forward = target.forward;
            Vector3 projectedForward = Vector3.ProjectOnPlane(forward, hit.normal).normalized;

            if (projectedForward.sqrMagnitude < 0.001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(projectedForward, hit.normal);

            target.rotation = Quaternion.Slerp(
                target.rotation,
                targetRotation,
                Time.fixedDeltaTime * alignSpeed
            );
        }
    }

}
