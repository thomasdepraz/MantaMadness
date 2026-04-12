using UnityEngine;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

public class SunHandler : MonoBehaviour
{
    
    public string[] goodAnimations;
    public string[] wackAnimations;

    public Animator sunAnimator;

    public void Start()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("MainMenu"))
            return;

        UIEffectManager.Instance.GoodAction += playGoodAnimation;
        UIEffectManager.Instance.BadAction += playWhackAnimation;
        UIEffectManager.Instance.SpecificAction += playSunAnimation;
        UIEffectManager.Instance.ExplosionAction += playExplosionAnimation;

    }

    public void playSunAnimation(UiParticles overload, string animName)
    {
        if(animName != null)
        sunAnimator.Play(animName);
    }

    public void playGoodAnimation()
    {
        sunAnimator.Play(goodAnimations[UnityEngine.Random.Range(0, goodAnimations.Length)]);
    }

    public void playWhackAnimation()
    {
        sunAnimator.Play(wackAnimations[UnityEngine.Random.Range(0, wackAnimations.Length)]);
    }

    public void playExplosionAnimation(string name)
    {
        sunAnimator.Play(name);
    }
}
