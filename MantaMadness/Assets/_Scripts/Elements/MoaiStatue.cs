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
    [SerializeField] private MoaiStatueCollisionRelay hitbox;
    [SerializeField] private int clamNumber;
    [SerializeField] private GameObject clam;
    [SerializeField] private bool hard = false;

    private bool isBroken = false;


    private void Start()
    {
        hitbox.HitCollision += StartMoaiDestruction;

        if (moaiVisual.activeSelf == false)
            moaiVisual.SetActive(true);

        if (moaiRemain.activeSelf == true)
            moaiRemain.SetActive(false);
    }

    void StartMoaiDestruction(float velocity)
    {
        if (isBroken == false)
        {
            if (!hard)
            {
                StartCoroutine(MoaiDestructionRoutine());
            }
            else if (hard)
            {
                if(velocity > Game.Instance.player.controllerData.maxSpeed)
                {
                    StartCoroutine(MoaiDestructionRoutine());
                }
            }

        }
    }

    private IEnumerator MoaiDestructionRoutine()
    {
        // PLAY PARTICLE AND DEACTIVATE VISUAL
        MoaiParticle.Play();
        //yield return new WaitForSeconds(0.1f);
        isBroken = true;
        moaiVisual.SetActive(false);
        moaiRemain.SetActive(true);


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
