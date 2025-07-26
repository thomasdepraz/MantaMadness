using UnityEngine;
using System;
using System.Collections;

public class SunHandler : MonoBehaviour
{
    
    public string[] goodAnimations;
    public string[] wackAnimations;

    public Animator sunAnimator;

    public void Start()
    {
        UIEffectManager.Instance.GoodAction += playGoodAnimation;
        UIEffectManager.Instance.BadAction += playWhackAnimation;
        UIEffectManager.Instance.SpecificAction += playSunAnimation;

    }

    public void playSunAnimation(string overload, string animName)
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
}
