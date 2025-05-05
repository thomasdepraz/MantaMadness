using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class Rail : MonoBehaviour
{
    public enum RailDirection
    {
        None,
        Forward,
        Backward
    }

    private SplineContainer splineContainer;
    private Spline railSpline;
    private float invRailLength;

    [Header("Rail parameters")]
    public CinemachineCamera railCamera;
    public float railSpeed;
    public RailDirection railDirection = RailDirection.None;

    public Vector3 Position => currentPosition;

    private Vector3 currentPosition;
    private float currentProgress = 0;
    private int dir;

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        railSpline = splineContainer.Spline;
        invRailLength = 1 / railSpline.GetLength();
        enabled = false;
    }

    public void EnterRail(Vector3 contactPosition, Vector3 velocity)
    {
        Vector3 nextPos = contactPosition + (velocity * Time.fixedDeltaTime);
        nextPos = transform.InverseTransformPoint(nextPos);
        contactPosition = transform.InverseTransformPoint(contactPosition);

        SplineUtility.GetNearestPoint(railSpline, contactPosition, out float3 nearest, out currentProgress);
        SplineUtility.GetNearestPoint(railSpline, nextPos, out _, out float nextT);

        switch (railDirection)
        {
            case RailDirection.None:
                dir = nextT > currentProgress ? 1 : -1;
                break;
            case RailDirection.Forward:
                dir = 1;
                break;
            case RailDirection.Backward:
                dir = -1;
                break;
        }

        currentPosition = transform.position + new Vector3(nearest.x, nearest.y, nearest.z);
        currentProgress = Mathf.Clamp01(currentProgress);
    }

    //return false when out
    public bool Progress(float deltaTime, out Vector3 position, out Vector3 normal, out Vector3 direction)
    {
        bool isIn = true;
        currentProgress += deltaTime * railSpeed * dir * invRailLength;//multiply by inv rail length
        if(dir < 0 && currentProgress <0)
        {
            isIn = false;
        }
        else if(dir > 0 && currentProgress > 1)
        {
            isIn = false;
        }

        currentProgress = Mathf.Clamp01(currentProgress);

        railSpline.Evaluate(currentProgress, out float3 pos, out float3 tan, out float3 up);
        
        position = transform.position + new Vector3(pos.x, pos.y, pos.z);
        direction = position - currentPosition;
        currentPosition = position;
        normal = new Vector3(up.x, up.y, up.z);

        return isIn;
    }
}
