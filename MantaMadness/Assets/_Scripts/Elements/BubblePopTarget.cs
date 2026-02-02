using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using DG.Tweening;

public class BubblePopTarget : JumpTarget
{
    [Header("Visual")]
    [SerializeField] private GameObject visual;
    [Header("Particles")]
    [SerializeField] protected ParticleSystem burstParticle;
    [Header("Parameters")]
    [SerializeField] protected float propulsionForce;
    protected override IEnumerator LaunchCoroutine()
    {
        player.BounceOnTarget(propulsionForce);
        burstParticle.Play();
        visual.SetActive(false);
        yield return new WaitForSeconds(respawnCooldown);
        visual.SetActive(true);
        visual.transform.localScale = Vector3.zero;
        visual.transform.DOScale(Vector3.one, 0.75f).SetEase(Ease.OutElastic);
        burstParticle.Play();
        launchRoutine = null;

    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SimpleController>() != null)
        {
            StartLaunchCoroutine();
            DeactivateTarget();
        }
    }
}
