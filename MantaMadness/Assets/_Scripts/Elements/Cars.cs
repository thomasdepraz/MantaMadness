using UnityEngine;
using UnityEngine.Splines;

public class Cars : MonoBehaviour
{
    public SplineAnimate spline;
    private AudioSource HornAudio;

    private void Awake()
    {
        HornAudio = gameObject.GetComponent<AudioSource>();
    }
    void Start()
    {
        spline.Play();
    }

    private void Update()
    {
        print(spline.IsPlaying);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            HornAudio.Play();
        }
    }
}
