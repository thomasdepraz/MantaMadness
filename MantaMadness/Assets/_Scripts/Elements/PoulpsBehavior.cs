using System.Collections;
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
    [SerializeField] private GameObject[] toActivate;
    [SerializeField] private Vector3 offset;

    [Header ("Poulp Visual + Anim")]
    [SerializeField] private GameObject visual;
    [SerializeField] private Animator animator;
    [SerializeField] private PoulpsRelay relay;

    private bool hasActivated =  false;

    private void Start()
    {
        if (toActivate.Length > 0) 
        {
            foreach (var item in toActivate)
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

        for (int i = 0; i < toActivate.Length; i++)
        {
            if(i == 0)
            {
                toActivate[i].gameObject.SetActive(true);
                toActivate[i].gameObject.transform.position = body.transform.position + offset;
                yield return new WaitForSeconds(splinePlayer.Duration / (toActivate.Length + 3));
            }        
            //else if (i == toActivate.Length - 2)
            //{
            //    toActivate[i].gameObject.SetActive(true);
            //    toActivate[i].gameObject.transform.position = body.transform.position;
            //    yield return new WaitForSeconds(splinePlayer.Duration / (toActivate.Length + 1));
            //}
            else
            {
                toActivate[i].gameObject.SetActive(true);
                toActivate[i].gameObject.transform.position = body.transform.position + offset;
                yield return new WaitForSeconds(splinePlayer.Duration / (toActivate.Length + 3));
            }
        }
        yield return null;
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(splinePlayer.Duration);
        splinePlayer.Pause();
        visual.SetActive(false);
        smokeInkParticles.Stop();
        smokeInkParticles.gameObject.SetActive(false);
    }

    private bool OnAnimationEvent = false;
    private IEnumerator Suprised()
    {
        sleepParticles.Stop();
        sleepParticles.gameObject.SetActive(false);
        exclamationParticles.Play();
        animator.SetTrigger("Suprised");
        yield return new WaitUntil(() => OnAnimationEvent);
        animator.SetTrigger("Sprint");
        smokeInkParticles.Play();
        OnAnimationEvent = false;
        StartCoroutine(spawnCoroutine());
        StartCoroutine(Timer());
    }

    private void AnimationEventTrigger()
    {
        OnAnimationEvent = true;
    }

}
