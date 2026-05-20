using DG.Tweening;
using FMODUnity;
using UnityEngine;
using System.Collections;

public class RespawnDestructible : Destructible
{
    [SerializeField] private float respawnTime = 10f;
    [SerializeField] private DestructibleCollisionRelay relay;
    public override void Start()
    {
        base.Start();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        relay = GetComponentInChildren<DestructibleCollisionRelay>();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void StartDestruction(Vector3 point)
    {
        if (isBroken == false)
        {
            StartCoroutine(DestructionRoutine(point));
        }
    }

    public override IEnumerator DestructionRoutine(Vector3 point)
    {
        //HITSTOP
        ImpactParticle.transform.position = point;
        ImpactParticle.Play();
        particle.Play();
        Game.Instance.player.triggerAnim.Invoke("HitStop");
        HitStopManager.instance.Stop(0.05f);
        isBroken = true;
        visual.SetActive(false);

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

                collectible.transform.position = transform.position + Vector3.up * 2f;

                collectible.transform.DOJump(collectible.transform.position + UnityEngine.Random.insideUnitSphere * 2f, 2f, 1, 0.4f);

                collectible.MoveToTarget(Game.Instance.player.gameObject);

                yield return new WaitForSeconds(0.15f);
            }
        }

        //END
        yield return null;

        yield return new WaitForSeconds(respawnTime);
        ReEnableDesructible();
        
    }

    public override void DisableDestructible()
    {
        visual.SetActive(false);
    }

    public void ReEnableDesructible()
    {
        isBroken = false;
        relay.GetComponent<Collider>().enabled = true;
        visual.SetActive(true);
    }
}
