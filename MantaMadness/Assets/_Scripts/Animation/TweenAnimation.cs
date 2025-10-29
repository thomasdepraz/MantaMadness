using UnityEngine;
using UnityEditor;
using DG.Tweening;

public class TweenAnimation : MonoBehaviour
{

    public bool playOnStart;
    public bool playOnEnable;
    public bool playOnBeat = false;

    [Header("Rotation Tween")]
    public bool animateRotation;
    public int loopRotationAmount = -1;
    public bool yoyoRotation = false;
    public float xRotation;
    public float yRotation;
    public float zRotation;
    public float rotationDuration;

    [Header("Position Tween")]
    public bool animatePosition;
    public int loopPositionAmount = -1;
    public bool yoyoPosition = false;
    public float xPos;
    public float yPos;
    public float zPos ;
    public float moveDuration;

    [Header("Scale Tween")]
    public bool animateScale;
    public int loopScaleAmount = -1;
    public bool yoyoScale = false;
    public float xScale = 1;
    public float yScale = 1;
    public float zScale = 1;
    public float scaleDuration;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Awake()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        
    }

    void Start()
    {
        if(playOnBeat == true)
        {
            MusicManager.OnBeat += BeatTween;
        }

        if (playOnStart == true)
            Tween();
    }

    public void OnEnable()
    {
        if (playOnBeat == true)
        {
            MusicManager.OnBeat += BeatTween;
        }

        if (playOnEnable == true)
            Tween();
    }

    public void OnDisable()
    {
        StopTween();
        MusicManager.OnBeat -= BeatTween;
    }

    public void OnDestroy()
    {
        MusicManager.OnBeat -= BeatTween;
    }

    public void Tween()
    {
        if(animateRotation == true)
        {
                if (yoyoRotation == false)
                    transform.DOLocalRotate(new Vector3(originalRotation.eulerAngles.x + xRotation, originalRotation.eulerAngles.y + yRotation, originalRotation.eulerAngles.z + zRotation), rotationDuration).SetEase(Ease.Linear).SetLoops(loopRotationAmount, LoopType.Incremental);
                else if (yoyoRotation == true)
                    transform.DOLocalRotate(new Vector3(originalRotation.eulerAngles.x + xRotation, originalRotation.eulerAngles.y + yRotation, originalRotation.eulerAngles.z + zRotation), rotationDuration).SetEase(Ease.InOutQuad).SetLoops(loopRotationAmount, LoopType.Yoyo);
        }
            

        if (animatePosition == true)
        {
            if (yoyoPosition == false)
                transform.DOLocalMove(new Vector3(originalPosition.x + xPos, originalPosition.y + yPos, originalPosition.z + zPos), moveDuration).SetEase(Ease.InOutQuad).SetLoops(loopPositionAmount, LoopType.Incremental);
            else if (yoyoPosition == true)
                transform.DOLocalMove(new Vector3(originalPosition.x + xPos, originalPosition.y + yPos, originalPosition.z + zPos), moveDuration).SetEase(Ease.InOutQuad).SetLoops(loopPositionAmount, LoopType.Yoyo);


        }

        if (animateScale == true)
        {
            if (yoyoScale == false)
            {
                if(originalScale == Vector3.zero)
                {
                    transform.DOScale(new Vector3(1 * xScale, 1 * yScale, 1 * zScale), scaleDuration).SetEase(Ease.InOutQuad).SetLoops(loopScaleAmount, LoopType.Incremental);
                }
                else
                {
                    transform.DOScale(new Vector3(originalScale.x * xScale, originalScale.y * yScale, originalScale.z * zScale), scaleDuration).SetEase(Ease.InOutQuad).SetLoops(loopScaleAmount, LoopType.Incremental);
                }
            }
 
            else if (yoyoScale == true)

                if (originalScale == Vector3.zero)
                {
                    transform.DOScale(new Vector3(1 * xScale, 1 * yScale, 1 * zScale), scaleDuration).SetEase(Ease.InOutQuad).SetLoops(loopScaleAmount, LoopType.Yoyo);
                }
                else
                {
                    transform.DOScale(new Vector3(originalScale.x * xScale, originalScale.y * yScale, originalScale.z * zScale), scaleDuration).SetEase(Ease.InOutQuad).SetLoops(loopScaleAmount, LoopType.Yoyo);
                }
            
        }
            

    }

    public void StopTween()
    {
        transform.DOKill(true);
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        transform.localScale = originalScale;
    }

    public void BeatTween(int bar, int beat, float tempo)
    {
        //print("bar =" + bar + " beat=" + beat + " tempo=" + tempo);
        //print(60 / tempo);
        StopTween();
        //print(tempo);
        transform.DOScale(new Vector3(originalScale.x * xScale, originalScale.y * yScale, originalScale.z * zScale), 60/tempo).SetEase(Ease.InOutQuad).SetLoops(loopScaleAmount, LoopType.Yoyo);
    }
}
