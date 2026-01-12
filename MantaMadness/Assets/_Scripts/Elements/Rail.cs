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

    public enum RailType
    {
        LIGHT,
        CONCRETE,
        WOOD,
        METAL
    }

    private SplineContainer splineContainer;
    private Spline railSpline;
    private float invRailLength;
    private float worldRailLength;

    [Header("Rail parameters")]
    public float railSpeed = 75f;
    public RailDirection railDirection = RailDirection.None;

    public Vector3 Position => currentPosition;

    private Vector3 currentPosition;
    private float currentProgress = 0;
    private int dir;

    public RailType railType;

    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        railSpline = splineContainer.Spline;
        //invRailLength = 1 / railSpline.GetLength();
        worldRailLength = ComputeWorldLength(railSpline, splineContainer.transform);
        invRailLength = 1f / worldRailLength;
        enabled = false;
    }

    private float ComputeWorldLength(Spline spline, Transform tf)
    {
        const int steps = 128; // smooth and cheap
        float length = 0f;

        spline.Evaluate(0f, out float3 prevPos, out _, out _);
        Vector3 prev = tf.TransformPoint(prevPos);

        for (int i = 1; i <= steps; i++)
        {
            float t = i / (float)steps;
            spline.Evaluate(t, out float3 p, out _, out _);
            Vector3 worldP = tf.TransformPoint(p);

            length += Vector3.Distance(prev, worldP);
            prev = worldP;
        }

        return length;
    }

    public void EnterRail(Vector3 contactPosition, Vector3 intentDirection)
    {
        contactPosition = transform.InverseTransformPoint(contactPosition);

        // Nearest spline point
        SplineUtility.GetNearestPoint(
            railSpline,
            contactPosition,
            out float3 nearest,
            out currentProgress
        );

        currentProgress = Mathf.Clamp01(currentProgress);

        // Tangente locale
        railSpline.Evaluate(currentProgress, out _, out float3 localTangent, out _);

        // Tangente world
        Vector3 worldTangent =
            splineContainer.transform.TransformDirection(localTangent).normalized;

        // Sécurité
        if (intentDirection.sqrMagnitude < 0.001f)
            intentDirection = worldTangent;

        intentDirection.Normalize();

        float dot = Vector3.Dot(intentDirection, worldTangent);

        switch (railDirection)
        {
            case RailDirection.None:
                dir = dot >= 0f ? 1 : -1;
                break;
            case RailDirection.Forward:
                dir = 1;
                break;
            case RailDirection.Backward:
                dir = -1;
                break;
        }

        currentPosition = splineContainer.transform.TransformPoint(nearest);
    }

    //return false when out
    public bool Progress(float deltaTime, out Vector3 position, out Vector3 normal, out Vector3 direction)
    {
        bool isClosed = railSpline.Closed;
        bool isIn = true;

        currentProgress += deltaTime * railSpeed * dir * invRailLength;

        if (isClosed)
        {
            // Boucle automatiquement entre 0 et 1
            currentProgress = Mathf.Repeat(currentProgress, 1f);
        }
        else
        {
            // Rail non fermé : sortie normale
            if (dir < 0 && currentProgress < 0f)
                isIn = false;
            else if (dir > 0 && currentProgress > 1f)
                isIn = false;

            currentProgress = Mathf.Clamp01(currentProgress);
        }

        railSpline.Evaluate(currentProgress, out float3 pos, out float3 tan, out float3 up);

        Vector3 worldPos = splineContainer.transform.TransformPoint(pos);
        Vector3 worldTan = splineContainer.transform.TransformDirection(tan);
        Vector3 worldUp = splineContainer.transform.TransformDirection(up);

        position = worldPos;
        direction = (worldPos - currentPosition);
        normal = worldUp;

        currentPosition = worldPos;

        return isIn;
    }
}
