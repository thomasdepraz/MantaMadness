using DG.Tweening;
using FMODUnity;
using System;
using System.Collections;
using UnityEngine;

public class Lilypad : MonoBehaviour
{
    [SerializeField] private GameObject visual;
    [SerializeField] private ParticleSystem[] bloomParticles;
    private bool hasBloomed = false;
    [SerializeField] private float scaleModifier = 1;
    private LilyPadManager manager;
    [SerializeField]private EventReference audioEvent;
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
 

        FMOD.Studio.EventInstance audio = RuntimeManager.CreateInstance(audioEvent);
        RuntimeManager.AttachInstanceToGameObject(audio,gameObject);
        audio.start();
        //ENABLE SUB VISUAL
        visual.SetActive(true);
        //TWEEN SSCALE OF MAIN VISUAL
        visual.transform.DOScale(visual.transform.localScale * scaleModifier,0.15f).SetEase(Ease.OutQuad).SetLoops(2,LoopType.Yoyo);

        //PARTICLE ACTIVATION
        foreach (ParticleSystem particle in bloomParticles)
        {
            particle.Play();
        }
    }

    public void AlternateBlooming()
    {
        if(hasBloomed == false)
        {
            hasBloomed = true;

            visual.SetActive(true);
            visual.transform.DOScale(visual.transform.localScale * scaleModifier, 0.15f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);

            foreach (ParticleSystem particle in bloomParticles)
            {
                particle.Play();
            }
        }
    }

    public void SetManager(LilyPadManager parentManager)
    {
        manager= parentManager;
    }
}
