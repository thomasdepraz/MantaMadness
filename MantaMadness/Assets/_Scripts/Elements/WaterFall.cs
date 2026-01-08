using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using FMODUnity;

public class WaterFall : MonoBehaviour
{
    [Header("Speed")]
    public bool useAcceleration = true;
    public float minSpeed = 5f;
    public float maxSpeed = 25f;
    public float acceleration = 15f;

    [Header("Splines")]
    public SplineContainer splineCurveContainer;
    public SplineContainer splineContainer;

    [Header("Camera")]
    public CinemachineCamera waterFallCamera;


    [Header("FMOD Sound")]
    public EventReference enterWaterfallCharge;
    public EventReference waterFallAppear;


    private bool hasStarted = false;

    private enum SplinePhase
    {
        Curve,
        Main
    }

    private SplinePhase currentPhase;

    private Spline currentSpline;
    private Transform currentSplineTransform;

    private float invSplineLength;
    private float currentProgress;
    private Vector3 currentPosition;

    private float currentSpeed;

    private void Awake()
    {
        SetActiveSpline(splineCurveContainer);
    }

    private void OnEnable()
    {
        if (!hasStarted)
            return;

        WaterFallAppear();
    }

    private void Start()
    {
        hasStarted = true;
    }

    public void EnterWaterFall()
    {
        ToggleWaterFallCamera(true);

        currentPhase = SplinePhase.Curve;
        SetActiveSpline(splineCurveContainer);

        currentProgress = 0f;
        currentSpeed = useAcceleration ? minSpeed : maxSpeed;

        currentSpline.Evaluate(0f, out float3 pos, out _, out _);
        currentPosition = currentSplineTransform.TransformPoint(pos);

        if (waterFallCamera != null)
            waterFallCamera.LookAt = Game.Instance.player.transform;

        RuntimeManager.PlayOneShot(enterWaterfallCharge, Game.Instance.player.transform.position);
    }

    public bool FollowSpline(
        float deltaTime,
        out Vector3 position,
        out Vector3 normal,
        out Vector3 direction)
    {
        bool isIn = true;

        if (useAcceleration)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                maxSpeed,
                acceleration * deltaTime
            );
        }

        currentProgress += deltaTime * currentSpeed * invSplineLength;

        if (currentProgress > 1f)
        {
            if (currentPhase == SplinePhase.Curve)
            {
                SwitchToMainSpline();
                currentProgress = 0f;
            }
            else
            {
                currentProgress = 1f;
                isIn = false;
            }
        }

        currentSpline.Evaluate(currentProgress, out float3 pos, out _, out float3 up);

        Vector3 worldPos = currentSplineTransform.TransformPoint(pos);
        Vector3 worldUp = currentSplineTransform.TransformDirection(up);

        position = worldPos;
        normal = worldUp;
        direction = worldPos - currentPosition;

        currentPosition = worldPos;

        return isIn;
    }

    private void SwitchToMainSpline()
    {
        currentPhase = SplinePhase.Main;
        SetActiveSpline(splineContainer);

        currentSpeed = maxSpeed;

        currentSpline.Evaluate(0f, out float3 pos, out _, out _);
        currentPosition = currentSplineTransform.TransformPoint(pos);
    }

    private void SetActiveSpline(SplineContainer container)
    {
        currentSpline = container.Spline;
        currentSplineTransform = container.transform;

        float worldLength = ComputeWorldLength(currentSpline, currentSplineTransform);
        invSplineLength = 1f / Mathf.Max(worldLength, 0.001f);
    }

    private float ComputeWorldLength(Spline spline, Transform tf)
    {
        const int steps = 128;
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

    public void ToggleWaterFallCamera(bool toggleValue)
    {
        if (waterFallCamera != null)
            waterFallCamera.enabled = toggleValue;
    }

    public void WaterFallAppear()
    {
        RuntimeManager.PlayOneShot(waterFallAppear, Game.Instance.player.transform.position);
    }
}
