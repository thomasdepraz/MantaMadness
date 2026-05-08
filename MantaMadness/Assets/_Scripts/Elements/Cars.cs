using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class Cars : MonoBehaviour
{
    protected AudioSource HornAudio;

    public SplineContainer splineRoad;
    [SerializeField] protected Spline trackSpline;

    [SerializeField] protected float acceleration = 5f;
    [SerializeField] protected Rigidbody rb;

    protected virtual void Awake()
    {
        HornAudio = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        trackSpline = splineRoad.Spline;
    }

    protected virtual void FixedUpdate()
    {
        var native = new NativeSpline(trackSpline);
        float distance = SplineUtility.GetNearestPoint(native, transform.position, out float3 nearest, out float t);

        Vector3 forward = Vector3.Normalize(splineRoad.EvaluateTangent(t));
        Vector3 up = splineRoad.EvaluateUpVector(t);

        var remappedForward = new Vector3(0, 1, 0);
        var remappedUp = new Vector3(0, 0, 1);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remappedUp));

        transform.rotation = Quaternion.LookRotation(forward, up) * axisRemapRotation;

        rb.linearVelocity = rb.linearVelocity.magnitude * transform.up;
        rb.AddForce(transform.up * acceleration);
    }

    protected virtual void OnCarDestroyed()
    {
        // comportement par défaut (vide)
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            HornAudio.Play();
        }
    }
}