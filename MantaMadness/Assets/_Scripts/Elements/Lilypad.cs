using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent (typeof(SphereCollider))]
public class Lilypad : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    [SerializeField] private ParticleSystem bloomParticle;
    private bool hasBloomed = false;
    [SerializeField] private float scaleModifier = 1;
    private LilyPadManager manager;

    private void Start()
    {
        if(visual.activeSelf == true)
        {
            visual.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if(hasBloomed == false)
            {
                hasBloomed = true;

                Blooming();
                manager.Collect();
            }
        }
    }

    private void Blooming()
    {
        //PARTICLE ACTIVATION
        bloomParticle.Play();
        //ENABLE SUB VISUAL
        visual.SetActive(true);
        //TWEEN SSCALE OF MAIN VISUAL
        transform.DOScale(transform.localScale * scaleModifier,0.15f).SetEase(Ease.OutQuad).SetLoops(2,LoopType.Yoyo);
    }

    public void SetManager(LilyPadManager parentManager)
    {
        manager= parentManager;
    }
}
