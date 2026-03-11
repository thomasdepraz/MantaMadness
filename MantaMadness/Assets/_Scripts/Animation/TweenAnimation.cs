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
    public bool scaleRelative;

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

    public bool playOnStart;
    public bool playOnEnable;
    public bool playSequenceOnBeat = false;
    public bool playSequenceOnEnable;

    [Header("Tween Steps Animations")]
    public List<TweenStep> tweenSteps = new List<TweenStep>();

    [Header("Sequence Settings")]
    public int sequenceLoops = 0;
    public LoopType sequenceLoopType = LoopType.Restart;
    public BeatType beatSequenceType;

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
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        transform.localScale = originalScale;
    }

    private Sequence currentSequence;

    public void PlaySequence()
    {
        StopTween();

        if (tweenSteps == null || tweenSteps.Count == 0)
            return;

        currentSequence = DOTween.Sequence();

        foreach (TweenStep step in tweenSteps)
        {
            Sequence stepSequence = DOTween.Sequence().SetAutoKill(false);

            // Delay de la step
            //if (step.delay > 0)
            //    stepSequence.PrependInterval(step.delay);

            if (step.animatePosition)
            {
                Tween moveTween = step.positionUseLocal
                    ? transform.DOLocalMove(step.position, step.duration)
                    : transform.DOMove(step.position, step.duration);

                if (step.positionRelative)
                    moveTween.SetRelative();

                moveTween.SetEase(step.ease);

                stepSequence.Join(moveTween);
            }

            if (step.animateRotation)
            {
                Tween rotateTween = step.rotationUseLocal
                    ? transform.DOLocalRotate(step.rotation, step.duration)
                    : transform.DORotate(step.rotation, step.duration);

                if (step.rotationRelative)
                    rotateTween.SetRelative();

                rotateTween.SetEase(step.ease);

                stepSequence.Join(rotateTween);
            }

            if (step.animateScale)
            {
                Tween scaleTween = transform.DOScale(step.scale, step.duration);

                if (step.scaleRelative)
                    scaleTween.SetRelative();

                scaleTween.SetEase(step.ease);

                stepSequence.Join(scaleTween);
            }
            currentSequence.AppendInterval(step.delay);
            currentSequence.Append(stepSequence);
        }


        currentSequence.SetLoops(sequenceLoops, sequenceLoopType);
        currentSequence.SetLink(gameObject);
    }

    public void PlayBeatSequence(int bar, int beat, float tempo)
    {
        if (tweenSteps == null || tweenSteps.Count == 0)
            return;

        TweenStep step = tweenSteps[currentBeatStep];

        float duration = 60f / tempo;

        if (step.animatePosition)
        {
            Tween moveTween = step.positionUseLocal
                ? transform.DOLocalMove(step.position, duration)
                : transform.DOMove(step.position, duration);

            if (step.positionRelative)
                moveTween.SetRelative();

            moveTween.SetEase(step.ease);
        }

        if (step.animateRotation)
        {
            Tween rotateTween = step.rotationUseLocal
                ? transform.DOLocalRotate(step.rotation, duration)
                : transform.DORotate(step.rotation, duration);

            if (step.rotationRelative)
                rotateTween.SetRelative();

            rotateTween.SetEase(step.ease);
        }

        if (step.animateScale)
        {
            Tween scaleTween = transform.DOScale(step.scale, duration);

            if (step.scaleRelative)
                scaleTween.SetRelative();

            scaleTween.SetEase(step.ease);
        }

        currentBeatStep++;

        if (currentBeatStep >= tweenSteps.Count)
        {
            currentBeatStep = 0;
        }
    }
}
