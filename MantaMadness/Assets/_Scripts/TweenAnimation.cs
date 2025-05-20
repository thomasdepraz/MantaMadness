using UnityEngine;
using UnityEditor;
using DG.Tweening;

public class TweenAnimation : MonoBehaviour
{

    public bool playOnStart;

    [Header("Rotation Tween")]
    public bool animateRotation;
    public float xRotation;
    public float yRotation;
    public float zRotation;
    public float rotationDuration;

    [Header("Position Tween")]
    public bool animatePosition;
    public float xPos;
    public float yPos;
    public float zPos ;
    public float moveDuration;

    [Header("Scale Tween")]
    public bool animateScale;
    public float xScale;
    public float yScale;
    public float zScale;
    public float scaleDuration;

    void Start()
    {
        if (playOnStart == true)
            Tween();
    }

    public void Tween()
    {
        if(animateRotation == true)
            transform.DORotate(new Vector3(xRotation,yRotation, zRotation), rotationDuration).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);

        if (animatePosition == true)
            transform.DOLocalMove(new Vector3(xPos, yPos, zPos), moveDuration).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);

        if (animateScale == true)
            transform.DOScale(new Vector3(xScale, yScale, zScale), scaleDuration).SetEase(Ease.InOutQuad).SetLoops(-1);
    }
}
