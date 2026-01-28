using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class JumpTarget : MonoBehaviour
{
    protected SimpleController player;
    [Header("Collision Layer")]
    [SerializeField] LayerMask playerMask;
    [Header("Particles")]
    [SerializeField] protected ParticleSystem indicator;
    [Header("Parameters")]
    [SerializeField] protected float respawnCooldown = 1f;

    public virtual event Action<SimpleController, Vector3> OnPlayerHit;

    protected virtual void NotifyPlayerHit(SimpleController p, Vector3 contactPoint)
    {
        OnPlayerHit?.Invoke(p, contactPoint);
    }


    protected virtual void Start()
    {
        player = Game.Instance.player;
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

    protected IEnumerator DisableCoroutine()
    {
        if (CameraTargetDetection.Instance.validJumpTargets.Contains(gameObject.GetComponent<Collider>()))
            {
                CameraTargetDetection.Instance.validJumpTargets.Remove(gameObject.GetComponent<Collider>());
                print(gameObject.GetComponent<Collider>() + "Has been removed");
            }
        ToggleFunctionElements(false);
        yield return new WaitForSeconds(respawnCooldown);
        ToggleFunctionElements(true);
        yield return null;
    }

    protected virtual void ToggleFunctionElements(bool toggleValue)
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

    protected bool OnAnimationEvent = false;
    protected virtual IEnumerator LaunchCoroutine()
    {
        yield return null;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<SimpleController>() != null)
        {
            DeactivateTarget();
        }
    }
}
