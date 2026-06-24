using UnityEngine;
using UnityEditor;
using DG.Tweening;
using System.Collections.Generic;
using NUnit.Framework;

[System.Serializable]
public struct TweenStep
{
    [Header("Enable")]
    public bool animatePosition;
    public bool animateRotation;
    public bool animateScale;

    [Header("Position")]
    public Vector3 position;
    public bool positionUseLocal;
    public bool positionRelative;

    [Header("Rotation")]
    public Vector3 rotation;
    public bool rotationUseLocal;
    public bool rotationRelative;

    [Header("Scale")]
    public Vector3 scale;

    [Header("Timing")]
    public float duration;
    public float delay;

    [Header("Tween Settings")]
    public Ease ease;
}

public enum BeatType
{
    beat,
    beat2,
    beat4,
    beat8,
}

public class TweenAnimation : MonoBehaviour
{
    public bool playSequenceOnBeat = false;
    public bool playSequenceOnEnable;

    [Header("Tween Steps Animations")]
    public List<TweenStep> tweenSteps = new List<TweenStep>();

    [Header("Sequence Settings")]
    public int sequenceLoops = 0;
    public LoopType sequenceLoopType = LoopType.Restart;
    public BeatType beatSequenceType;

    [Header("Time Settings")]
    public bool useUnscaledTime = false;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private int currentBeatStep = 0;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

    }

    void Start()
    {
        //if (playOnBeat == true)
        //{
        //    currentBeatStep = 0;

        //    if ( beatSequenceType == BeatType.beat)
        //    {
        //        MusicManager.OnBeat += PlayBeatSequence;
        //    }
        //    else if(beatSequenceType == BeatType.beat2)
        //    {
        //        MusicManager.OnBeat2 += PlayBeatSequence;
        //    }
        //    else if (beatSequenceType == BeatType.beat4)
        //    {
        //        MusicManager.OnBeat4 += PlayBeatSequence;
        //    }
        //    else if (beatSequenceType == BeatType.beat8)
        //    {
        //        MusicManager.OnBeat8 += PlayBeatSequence;
        //    }
        //}
    }

    public void OnEnable()
    {
        if (playSequenceOnBeat == true)
        {
            currentBeatStep = 0;

            switch (beatSequenceType)
            {
                case BeatType.beat:
                    MusicManager.OnBeat += PlayBeatSequence;
                    break;
                case BeatType.beat2:
                    MusicManager.OnBeat2 += PlayBeatSequence;
                    break;
                case BeatType.beat4:
                    MusicManager.OnBeat4 += PlayBeatSequence;
                    break;
                case BeatType.beat8:
                    MusicManager.OnBeat8 += PlayBeatSequence;
                    break;
            }
        }
        if (playSequenceOnEnable == true)
            PlaySequence();
    }

    public void OnDisable()
    {
        StopTween();

        switch (beatSequenceType)
        {
            case BeatType.beat:
                MusicManager.OnBeat -= PlayBeatSequence;
                break;
            case BeatType.beat2:
                MusicManager.OnBeat2 -= PlayBeatSequence;
                break;
            case BeatType.beat4:
                MusicManager.OnBeat4 -= PlayBeatSequence;
                break;
            case BeatType.beat8:
                MusicManager.OnBeat8 -= PlayBeatSequence;
                break;
        }
    }

    public void OnDestroy()
    {
        switch (beatSequenceType)
        {
            case BeatType.beat:
                MusicManager.OnBeat -= PlayBeatSequence;
                break;
            case BeatType.beat2:
                MusicManager.OnBeat2 -= PlayBeatSequence;
                break;
            case BeatType.beat4:
                MusicManager.OnBeat4 -= PlayBeatSequence;
                break;
            case BeatType.beat8:
                MusicManager.OnBeat8 -= PlayBeatSequence;
                break;
        }
    }

    public void StopTween()
    {
        transform.DOKill(true);

        if (sequenceLoopType != LoopType.Incremental)
        {
            transform.localPosition = originalPosition;
            transform.localRotation = originalRotation;
            transform.localScale = originalScale;
        }
    }

    private Sequence currentSequence;

    public void PlaySequence()
    {
        StopTween();

        if (tweenSteps == null || tweenSteps.Count == 0)
            return;

        currentSequence = DOTween.Sequence().SetUpdate(UpdateType.Normal, useUnscaledTime);

        Vector3 simulatedScale = transform.localScale;

        foreach (TweenStep step in tweenSteps)
        {
            if (step.delay > 0)
                currentSequence.AppendInterval(step.delay);

            float tweenDuration = step.duration;
            Tween stepTween = null;

            if (step.animateRotation)
            {
                stepTween = step.rotationUseLocal
                    ? transform.DOLocalRotate(step.rotation, tweenDuration, RotateMode.FastBeyond360)
                    : transform.DORotate(step.rotation, tweenDuration, RotateMode.FastBeyond360);

                if (step.rotationRelative)
                    stepTween.SetRelative();
            }
            else if (step.animatePosition)
            {
                stepTween = step.positionUseLocal
                    ? transform.DOLocalMove(step.position, tweenDuration)
                    : transform.DOMove(step.position, tweenDuration);

                if (step.positionRelative)
                    stepTween.SetRelative();
            }
            else if (step.animateScale)
            {
                Vector3 baseScale = sequenceLoopType == LoopType.Restart
                    ? originalScale
                    : transform.localScale;

                Vector3 targetScale = Vector3.Scale(baseScale, step.scale);

                stepTween = transform.DOScale(targetScale, tweenDuration);
            }

            if (stepTween != null)
            {
                stepTween.SetEase(step.ease);
                currentSequence.Append(stepTween);
            }
        }

        currentSequence.SetLoops(sequenceLoops, sequenceLoopType);
        currentSequence.SetLink(gameObject);
    }

    public void PlayBeatSequence(int bar, int beat, float tempo)
    {
        if (tweenSteps == null || tweenSteps.Count == 0)
            return;

        if (Time.timeScale == 0f && !useUnscaledTime)
        {
            currentBeatStep++;
            if (currentBeatStep >= tweenSteps.Count)
                currentBeatStep = 0;
            return;
        }

        transform.DOKill();

        TweenStep step = tweenSteps[currentBeatStep];

        float beatDuration = 60f / tempo;
        float tweenDuration = sequenceLoopType == LoopType.Yoyo ? beatDuration * 0.5f : beatDuration;

        if (step.animatePosition)
        {
            Tween moveTween = step.positionUseLocal
                ? transform.DOLocalMove(step.position, tweenDuration)
                : transform.DOMove(step.position, tweenDuration);

            if (step.positionRelative)
                moveTween.SetRelative();

            moveTween.SetEase(step.ease);
            moveTween.SetUpdate(useUnscaledTime);

            if (sequenceLoopType == LoopType.Yoyo)
                moveTween.SetLoops(2, LoopType.Yoyo);
        }

        if (step.animateRotation)
        {
            Tween rotateTween = step.rotationUseLocal
                ? transform.DOLocalRotate(step.rotation, tweenDuration, RotateMode.FastBeyond360)
                : transform.DORotate(step.rotation, tweenDuration, RotateMode.FastBeyond360);

            if (step.rotationRelative)
                rotateTween.SetRelative();

            rotateTween.SetEase(step.ease);
            rotateTween.SetUpdate(useUnscaledTime);

            if (sequenceLoopType == LoopType.Yoyo)
                rotateTween.SetLoops(2, LoopType.Yoyo);
        }

        if (step.animateScale)
        {
            Vector3 baseScale = sequenceLoopType == LoopType.Restart
                ? originalScale
                : transform.localScale;

            Vector3 targetScale = Vector3.Scale(baseScale, step.scale);

            if (sequenceLoopType == LoopType.Restart)
                transform.localScale = baseScale;

            Tween scaleTween = transform.DOScale(targetScale, tweenDuration);

            scaleTween.SetEase(step.ease);
            scaleTween.SetUpdate(useUnscaledTime);

            if (sequenceLoopType == LoopType.Yoyo)
                scaleTween.SetLoops(2, LoopType.Yoyo);
        }

        currentBeatStep++;
        if (currentBeatStep >= tweenSteps.Count)
            currentBeatStep = 0;
    }
}
