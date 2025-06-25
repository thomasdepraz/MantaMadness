using UnityEngine;

public class SunHandler : MonoBehaviour
{

    public string[] goodAnimations;
    public string[] wackAnimations;

    public Animator sunAnimator;

    public void Start()
    {
        UIManager.Instance.sunInterface = this;
    }

    public void playSunAnimation(string animName)
    {
        sunAnimator.Play(animName);
    }

    public void playGoodAnimation()
    {
        sunAnimator.Play(goodAnimations[Random.Range(0, goodAnimations.Length)]);
    }

    public void playWhackAnimation()
    {
        sunAnimator.Play(wackAnimations[Random.Range(0, wackAnimations.Length)]);
    }
}
