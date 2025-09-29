using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class JumpTarget : MonoBehaviour
{
    private SimpleController player;
    [Header("Collision Layer")]
    [SerializeField] LayerMask playerMask;
    [Header("Particles")]
    [SerializeField] private ParticleSystem indicator;
    [SerializeField] private ParticleSystem burstParticle;
    [Header("Parameters")]
    [SerializeField] private float respawnCooldown = 1f; 
    [SerializeField] private float propulsionForce;
    [Header("Animation")]
    [SerializeField] private Animator animator;
    [Header("Camera")]
    [SerializeField] private CinemachineCamera targetCam;

    private void Start()
    {
        player = Game.Instance.player;
        targetCam.enabled = false;
    }
    
    public void SwitchIndicatorVisibility(bool validTarget)
    {
        if (!validTarget)
        {
            indicator.Stop();
            indicator.gameObject.SetActive(false);

        }
        else if (validTarget)
        {
            indicator.gameObject.SetActive(true);
            indicator.Play();
        }
    }

    public void DeactivateTarget()
    {
        StartCoroutine(DisableCoroutine());
    }

    private IEnumerator DisableCoroutine()
    {
        if (CameraTargetDetection.Instance.validTargets.Contains(gameObject.GetComponent<Collider>()))
            {
                CameraTargetDetection.Instance.validTargets.Remove(gameObject.GetComponent<Collider>());
                print(gameObject.GetComponent<Collider>() + "Has been removed");
            }
        ToggleFunctionElements(false);
        yield return new WaitForSeconds(respawnCooldown);
        ToggleFunctionElements(true);
        yield return null;
    }

    private void ToggleFunctionElements(bool toggleValue)
    {
        if (toggleValue)
        {
            //SET ANIMATION TO IDLE
            gameObject.GetComponent<Collider>().enabled = true;
            indicator.gameObject.SetActive(true);
        }
        else if (!toggleValue)
        {
            //SET ANIMATION TO DISABLE
            gameObject.GetComponent<Collider>().enabled = false;
            indicator.gameObject.SetActive(false);
        }

    }

    public void StartLaunchCoroutine()
    {
        StartCoroutine(LaunchCoroutine());
    }

    private bool OnAnimationEvent = false;
    private IEnumerator LaunchCoroutine()
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

    private void AnimationEventTrigger()
    {
        OnAnimationEvent = true;
    }

}
