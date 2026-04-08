using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public enum UiParticles
{
    NULL,
    SICK,
    SHEEESH,
    SEXY,
    MEGACLAM,
    JOHNNYFOUND,
    WOW,
    BOOM,
    KABOOM,
    CHALLENGE,
    BUBBLE,
    MISSINGHAND,
    CHECKPOINT,
    SPEEDLINE,
}

public enum WordType
{
    WOW,
    SEXY,
    SICK,
    NULL,
}

[Serializable]
public struct UiParticleStruct
{
    public UiParticles type;
    public VFXData data;
    public EventReference sound;
    public string param;
    public WordType paramValue;
    public bool doesntPlaySound;
}

public class UIParticleManager : MonoBehaviour
{
    public static UIParticleManager Instance;
    [SerializeField] private List<UiParticleStruct> uiWordParticleList = new();
    [SerializeField] private List<UiParticleStruct> uiParticleGood;
    [SerializeField] private List<UiParticleStruct> uiParticleExplosion;

    private bool explosionInCooldown = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        UIEffectManager.Instance.GoodAction += playGoodParticle;
        UIEffectManager.Instance.SpecificAction += playSpecificUIParticle;
        UIEffectManager.Instance.ExplosionAction += playExplosionParticle;
    }

    private void OnDisable()
    {
        UIEffectManager.Instance.GoodAction -= playGoodParticle;
        UIEffectManager.Instance.SpecificAction -= playSpecificUIParticle;
        UIEffectManager.Instance.ExplosionAction -= playExplosionParticle;
    }

    public void playSpecificUIParticle(UiParticles word, string overload)
    {
        for (int i = 0; i < uiWordParticleList.Count; i++)
        {
            if (uiWordParticleList[i].type == word)
            {
                uiWordParticleList[i].data.PlayParticle();
                if(!uiWordParticleList[i].sound.IsNull && !uiWordParticleList[i].doesntPlaySound)
                    PlayParticleSound(uiWordParticleList[i].sound, uiWordParticleList[i].param, uiWordParticleList[i].paramValue);

                break;
            }
        }
    }

    public void stopSpecificUIParticle(UiParticles word, string overload)
    {
        for (int i = 0; i < uiWordParticleList.Count; i++)
        {
            if (uiWordParticleList[i].type == word)
            {
                uiWordParticleList[i].data.StopParticle();

                break;
            }
        }
    }

    public void playGoodParticle()
    {
        UiParticleStruct particle = uiParticleGood[UnityEngine.Random.Range(0, uiParticleGood.Count)];
        particle.data.PlayParticle();
        if (!particle.sound.IsNull && !particle.doesntPlaySound)
            PlayParticleSound(particle.sound, particle.param, particle.paramValue);
    }
    
    public void playExplosionParticle(string overload)
    {
        StartCoroutine(ExplosionParticleCoroutine());
    }

    private Coroutine explosionCooldownRoutine;

    private IEnumerator ExplosionCooldownCoroutine()
    {
        yield return new WaitForSeconds(3f);
        explosionCooldownRoutine = null;
    }

    public IEnumerator ExplosionParticleCoroutine()
    {
        if(explosionCooldownRoutine == null)
        {
            explosionCooldownRoutine = StartCoroutine(ExplosionCooldownCoroutine());
            UiParticleStruct particle = uiParticleGood[UnityEngine.Random.Range(0, uiParticleExplosion.Count)];
            particle.data.PlayParticle();
            if (!particle.sound.IsNull && !particle.doesntPlaySound)
                PlayParticleSound(particle.sound, particle.param, particle.paramValue);
            yield return null;
        }
    }

    public void PlayParticleSound(EventReference soundToPlay, string fmodParameter, WordType parameterValue)
    {
        EventInstance instance = RuntimeManager.CreateInstance(soundToPlay);

        //IMPORTANT FOR SFX WITH FMOD LOCAL PARAMETER
        if (fmodParameter != null && parameterValue != WordType.NULL)
        {
            instance.setParameterByName(fmodParameter, (float)parameterValue);
        }
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(Game.Instance.player.transform.position));
        instance.start();
        instance.release();
    }
}
