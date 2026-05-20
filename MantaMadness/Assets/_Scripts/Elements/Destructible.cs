using DG.Tweening;
using FMODUnity;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    [SerializeField] protected GameObject visual;
    [SerializeField] protected GameObject remain;
    [SerializeField] protected ParticleSystem particle;
    [SerializeField] protected ParticleSystem ImpactParticle;
    [SerializeField] protected DestructibleCollisionRelay hitbox;
    [SerializeField] protected bool hard = false;
    [SerializeField] protected EventReference destructionSFX;

    [SerializeField] protected Collectible[] collectibleRewards;

    public bool isBroken = false;


    public virtual void Start()
    {
        if (visual.activeSelf == false)
            visual.SetActive(true);

        if(remain != null)
        {
            if (remain.activeSelf == true)
                remain.SetActive(false);
        }

        collectibleRewards = GetComponentsInChildren<Collectible>(true);
    }

    public virtual void OnEnable()
    {
        hitbox.HitCollision += StartDestruction;
    }

    public virtual void OnDisable()
    {
        hitbox.HitCollision -= StartDestruction;
    }

    public virtual void StartDestruction(Vector3 point)
    {
        if (isBroken == false)
        {
            StartCoroutine(DestructionRoutine(point));
        }
    }

    public virtual IEnumerator DestructionRoutine(Vector3 point)
    {
        //HITSTOP
        ImpactParticle.transform.position = point;
        ImpactParticle.Play();
        particle.Play();
        Game.Instance.player.triggerAnim.Invoke("HitStop");
        HitStopManager.instance.Stop(0.05f);
        // PLAY PARTICLE AND DEACTIVATE VISUAL
        //yield return new WaitForSeconds(0.1f);
        isBroken = true;
        visual.SetActive(false);
        //moaiRemain.SetActive(true);

        //SOUND PLAY
        RuntimeManager.PlayOneShot(destructionSFX, transform.position);


        //START SPAWNING CLAMS
        for (int i = 0; i < collectibleRewards.Length; i++)
        {
            Collectible collectible = collectibleRewards[i];

            if (collectible == null)
                continue;

            if (collectible.State == CollectibleState.Activable)
            {
                collectible.ActivateCollectible();

                collectible.transform.position =transform.position + Vector3.up * 2f;

                collectible.transform.DOJump(collectible.transform.position +UnityEngine.Random.insideUnitSphere * 2f,2f,1,0.4f);

                collectible.MoveToTarget(Game.Instance.player.gameObject);

                yield return new WaitForSeconds(0.15f);
            }
        }

        //END
        yield return null;
    }

    public virtual void DisableDestructible()
    {
        Debug.Log("Bha bonhomme ca va pas ?");
        visual.SetActive(false);
    }
}
