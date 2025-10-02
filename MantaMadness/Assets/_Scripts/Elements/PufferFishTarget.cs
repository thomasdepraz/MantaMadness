using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PufferFishTarget : JumpTarget
{
    [SerializeField] protected ParticleSystem burstParticle;
    [Header("Parameters")]
    [SerializeField] protected float propulsionForce;
    [Header("Animation")]
    [SerializeField] protected Animator animator;
    [Header("Camera")]
    [SerializeField] protected CinemachineCamera targetCam;

    protected override void Start()
    {
        base.Start();
        targetCam.enabled = false;
    }

    protected override IEnumerator LaunchCoroutine()
    {
        animator.SetTrigger("Trigger");
        targetCam.enabled = true;
        yield return new WaitUntil(() => OnAnimationEvent);
        targetCam.enabled = false;
        OnAnimationEvent = false;
        player.togglePlayerBodyVisual(true);
        player.PropelledByTarget(transform, propulsionForce);
        burstParticle.Play();
    }
    public void AnimationEventTrigger()
    {
        OnAnimationEvent = true;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SimpleController>() != null)
        {
            DeactivateTarget();
            other.GetComponent<SimpleController>().StopByTargetImpact(gameObject);
        }
    }
}
