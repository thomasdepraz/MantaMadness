using DG.Tweening;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class MoaiStatue : MonoBehaviour
{
    [SerializeField] private GameObject moaiVisual;
    [SerializeField] private ParticleSystem MoaiParticle;
    [SerializeField] private MoaiStatueCollisionRelay hitbox;
    [SerializeField] private int clamNumber;
    [SerializeField] private GameObject clam;

    private bool isBroken = false;

    private void Start()
    {
        hitbox.HitCollision += StartMoaiDestruction;

        if (moaiVisual.activeSelf == false)
            moaiVisual.SetActive(true);
    }

    void StartMoaiDestruction()
    {
        if (isBroken == false)
        {
            StartCoroutine(MoaiDestructionRoutine());
        }
    }

    private IEnumerator MoaiDestructionRoutine()
    {
        // PLAY PARTICLE AND DEACTIVATE VISUAL
        MoaiParticle.Play();
        //yield return new WaitForSeconds(0.1f);
        isBroken = true;
        moaiVisual.SetActive(false);


        //START SPAWNING CLAMS
        for (int i = 0; i < clamNumber; i++) 
        {
            var radians = 2 * MathF.PI / clamNumber * (i + 1);
            var vertical = Mathf.Sin(radians);
            var horizontal = Mathf.Cos(radians);

            var spawnDir = new Vector3(horizontal, 0.25f, vertical);
            

            GameObject newClam = Instantiate(clam, transform.position + new Vector3(0,1,0), Quaternion.identity);
            var spawnPos = newClam.transform.position + spawnDir * 10f;
            newClam.transform.DOLocalMove(spawnPos, 1f);
            yield return new WaitForSeconds(0.1f);
        }

        //END
        yield return null;
    }
}
