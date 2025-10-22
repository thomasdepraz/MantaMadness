using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MoaiStatue : MonoBehaviour
{
    [SerializeField] private GameObject moaiVisual;
    [SerializeField] private GameObject moaiRemain;
    [SerializeField] private ParticleSystem MoaiParticle;
    [SerializeField] private ParticleSystem ImpactParticle;
    [SerializeField] private MoaiStatueCollisionRelay hitbox;
    [SerializeField] private int clamNumber;
    [SerializeField] private GameObject clam;
    [SerializeField] private bool hard = false;

    private bool isBroken = false;


    private void Start()
    {
        if (moaiVisual.activeSelf == false)
            moaiVisual.SetActive(true);

        if (moaiRemain.activeSelf == true)
            moaiRemain.SetActive(false);
    }

    private void OnEnable()
    {
        hitbox.HitCollision += StartMoaiDestruction;
    }

    private void OnDisable()
    {
        hitbox.HitCollision -= StartMoaiDestruction;
    }

    void StartMoaiDestruction(float velocity, Vector3 point)
    {
        if (isBroken == false)
        {
            if (!hard)
            {
                StartCoroutine(MoaiDestructionRoutine(point));
            }
            else if (hard)
            {
                if(velocity > Game.Instance.player.controllerData.maxSpeed)
                {
                    StartCoroutine(MoaiDestructionRoutine(point));
                }
            }

        }
    }

    private IEnumerator MoaiDestructionRoutine(Vector3 point)
    {
        //HITSTOP
        ImpactParticle.transform.position = point;
        ImpactParticle.Play();
        MoaiParticle.Play();
        Game.Instance.player.triggerAnim.Invoke("HitStop");
        HitStopManager.instance.Stop(0.05f);
        // PLAY PARTICLE AND DEACTIVATE VISUAL
        //yield return new WaitForSeconds(0.1f);
        isBroken = true;
        moaiVisual.SetActive(false);
        //moaiRemain.SetActive(true);


        //START SPAWNING CLAMS
        for (int i = 0; i < clamNumber; i++) 
        {
            //var radians = 2 * MathF.PI / clamNumber * (i + 1);
            //var vertical = Mathf.Sin(radians);
            //var horizontal = Mathf.Cos(radians);

            //var spawnDir = new Vector3(horizontal, 0.25f, vertical);
            

            GameObject newClam = Instantiate(clam, transform.position + new Vector3(0,5f,0), Quaternion.identity);
            //var spawnPos = newClam.transform.position + spawnDir * 10f;
            //newClam.transform.DOLocalMove(spawnPos, 1f);
            newClam.gameObject.GetComponent<Collectible>().MoveToTarget(Game.Instance.player.gameObject);
            yield return new WaitForSeconds(0.15f);
        }

        //END
        yield return null;
    }
}
