using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class JumpTarget : MonoBehaviour
{
    private SimpleController player;
    [SerializeField] LayerMask playerMask;

    [SerializeField] private ParticleSystem indicator;
    [SerializeField] private Material[] materials;
    [SerializeField] private float respawnCooldown = 1f; 
    [SerializeField] private float propulsionForce;
    [SerializeField] private Animator animator;
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

    }

    private void AnimationEventTrigger()
    {
        OnAnimationEvent = true;
    }

}
