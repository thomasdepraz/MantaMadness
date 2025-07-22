using UnityEngine;
using UnityEngine.Splines;

public class Cars : MonoBehaviour
{
    public SplineAnimate spline;

    void Start()
    {
        spline.Play();
    }

    private void Update()
    {
        print(spline.IsPlaying);
        spline.Play();
    }
}
