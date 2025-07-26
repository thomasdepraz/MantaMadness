using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class Cars : MonoBehaviour
{
    private AudioSource HornAudio;

    public SplineContainer splineRoad;
    [SerializeField] Spline trackSpline;

    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float acceleration = 5f;

    private float splineT = 0f;

    [SerializeField] private Rigidbody rb;


    private void Awake()
    {
        HornAudio = gameObject.GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        //spline.Play();
        trackSpline = splineRoad.Spline;
    }

    private void FixedUpdate()
    {

        var native = new NativeSpline(trackSpline);
        float distance = SplineUtility.GetNearestPoint(native, transform.position, out float3 nearest,out float t);


        Vector3 forward = Vector3.Normalize(splineRoad.EvaluateTangent(t));
        Vector3 up = splineRoad.EvaluateUpVector(t);

        var remappedForward = new Vector3(0,1,0);
        var remmapedUp = new Vector3(0, 0, 1);
        var axisRemapRotation = Quaternion.Inverse(Quaternion.LookRotation(remappedForward, remmapedUp));

        transform.rotation = Quaternion.LookRotation(forward, up) * axisRemapRotation;

        rb.linearVelocity = rb.linearVelocity.magnitude * transform.up;

        rb.AddForce(transform.up * acceleration);
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            HornAudio.Play();
        }
    }
}
