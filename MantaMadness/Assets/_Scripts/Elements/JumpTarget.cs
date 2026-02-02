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
    public bool isAvailable = true;

    public void DeactivateTarget()
    {
        if (!isAvailable) return;

        isAvailable = false;

        var col = GetComponent<Collider>();
        if (CameraTargetDetection.Instance != null && col != null)
            CameraTargetDetection.Instance.validJumpTargets.Remove(col);

        ToggleFunctionElements(false);

        StartCoroutine(DisableCoroutine());
    }

    protected IEnumerator DisableCoroutine()
    {
        //if (CameraTargetDetection.Instance.validJumpTargets.Contains(gameObject.GetComponent<Collider>()))
        //    {
        //        CameraTargetDetection.Instance.validJumpTargets.Remove(gameObject.GetComponent<Collider>());
        //        print(gameObject.GetComponent<Collider>() + "Has been removed");
        //    }
        //ToggleFunctionElements(false);

        yield return new WaitForSeconds(respawnCooldown);
        ToggleFunctionElements(true);
    }

    protected virtual void ToggleFunctionElements(bool toggleValue)
    {
        if (toggleValue)
        {
            //SET ANIMATION TO IDLE
            gameObject.GetComponent<Collider>().enabled = true;
            indicator.gameObject.SetActive(true);
            isAvailable = true;
        }
        else if (!toggleValue)
        {
            //SET ANIMATION TO DISABLE
            gameObject.GetComponent<Collider>().enabled = false;
            indicator.gameObject.SetActive(false);
            isAvailable = false;
        }
    }

    public void StartLaunchCoroutine()
    {
        if (launchRoutine != null || !isAvailable) return;
        launchRoutine = StartCoroutine(LaunchCoroutine());
    }

    protected bool OnAnimationEvent = false;

    public Coroutine launchRoutine;
    protected virtual IEnumerator LaunchCoroutine()
    {
        yield return null;
        launchRoutine = null;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<SimpleController>() != null)
        {
            //DeactivateTarget();
        }
    }
}
