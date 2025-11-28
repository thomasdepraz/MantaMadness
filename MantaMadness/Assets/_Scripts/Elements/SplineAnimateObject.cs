using FMODUnity;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineAnimate))]
public class SplineAnimateObject : MonoBehaviour
{
    private SplineAnimate splineAnimate;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float startPoint;

    private void Awake()
    {
        if (GetComponent<SplineAnimate>() != null)
        {
            splineAnimate = GetComponent<SplineAnimate>();
        }
    }

    void Start()
    {
        if (splineAnimate != null)
        {
            splineAnimate.StartOffset = startPoint;
            splineAnimate.Play();
        }
    }

    private void OnEnable()
    {
        if (splineAnimate != null)
        {
            splineAnimate.Play();
        }
    }
}
