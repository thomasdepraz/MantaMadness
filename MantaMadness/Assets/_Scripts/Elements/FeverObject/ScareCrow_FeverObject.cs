using FMODUnity;
using System.Collections;
using UnityEngine;

public class ScareCrow_FeverObject : FeverObject
{
    [SerializeField] private Animator scarecrowAnimator;
    [SerializeField] private EventReference audioEvent;

    private bool hasActivated = false;
    private bool waitingForBeat = false;

    protected override IEnumerator DelaySetup()
    {
        OnFeverReset();
        MusicManager.OnBeat += PlayAnimationOnBeat;
        yield return base.DelaySetup();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        MusicManager.OnBeat -= PlayAnimationOnBeat;
    }

    public override void OnFeverRange()
    {
        if (hasActivated || waitingForBeat)
            return;

        StartCoroutine(WaitForNextBeat());
    }

    IEnumerator WaitForNextBeat()
    {
        waitingForBeat = true;

        bool beatReceived = false;

        void OnBeat(int bar, int beat, float tempo)
        {
            beatReceived = true;
        }

        MusicManager.OnBeat += OnBeat;

        yield return new WaitUntil(() => beatReceived);

        MusicManager.OnBeat -= OnBeat;

        ActivateFlower();

        waitingForBeat = false;
    }

    void ActivateFlower()
    {
        hasActivated = true;

        FMOD.Studio.EventInstance audio = RuntimeManager.CreateInstance(audioEvent);
        RuntimeManager.AttachInstanceToGameObject(audio, gameObject);
        audio.start();
        audio.release();

        PlayAnimation("fever");
    }

    public override void OnFeverReset()
    {
        if (!hasActivated)
            PlayAnimation("reset");
    }

    private void PlayAnimation(string name)
    {
        scarecrowAnimator.SetTrigger(name);
    }

    private void PlayAnimationOnBeat(int bar, int beat, float tempo)
    {
        if (hasActivated)
            PlayAnimation("onBeat");
    }
}
