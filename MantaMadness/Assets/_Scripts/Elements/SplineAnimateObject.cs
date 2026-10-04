using FMODUnity;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineAnimate))]
public class SplineAnimateObject : MonoBehaviour
{
    private SplineAnimate splineAnimate;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float startPoint;

    private Renderer[] _renderers;
    private bool[] _rendererWasEnabled;
    private Collider[] _colliders;
    private bool[] _colliderWasEnabled;
    private bool _isDistanceCulled;
    private float _nextCullCheckTime;

    private void Awake()
    {
        if (GetComponent<SplineAnimate>() != null)
        {
            splineAnimate = GetComponent<SplineAnimate>();
        }
    }

    void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>(true);
        _rendererWasEnabled = new bool[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _rendererWasEnabled[i] = _renderers[i].enabled;

        _colliders = GetComponentsInChildren<Collider>(true);
        _colliderWasEnabled = new bool[_colliders.Length];
        for (int i = 0; i < _colliders.Length; i++)
            _colliderWasEnabled[i] = _colliders[i].enabled;

        if (splineAnimate != null)
        {
            splineAnimate.StartOffset = startPoint;
            splineAnimate.Play();
        }
    }

    private void Update()
    {
        if (Time.time < _nextCullCheckTime)
            return;

        _nextCullCheckTime = Time.time + DistanceCullBand.CHECK_INTERVAL;

        if (splineAnimate == null)
            return;

        if (DistanceCullBand.TryGetPlayerPosition(out Vector3 playerPosition) == false)
            return;

        float sqrDistance = (transform.position - playerPosition).sqrMagnitude;
        float pauseDistanceSqr = DistanceCullBand.PAUSE_DISTANCE * DistanceCullBand.PAUSE_DISTANCE;
        float resumeDistanceSqr = DistanceCullBand.RESUME_DISTANCE * DistanceCullBand.RESUME_DISTANCE;

        if (_isDistanceCulled == false && sqrDistance > pauseDistanceSqr)
            ApplyDistanceCull();
        else if (_isDistanceCulled && sqrDistance < resumeDistanceSqr)
            ClearDistanceCull();
    }

    private void ApplyDistanceCull()
    {
        _isDistanceCulled = true;
        splineAnimate.Pause();
        splineAnimate.enabled = false;
        SetRenderersAndCollidersEnabled(false);
    }

    private void ClearDistanceCull()
    {
        _isDistanceCulled = false;
        SetRenderersAndCollidersEnabled(true);
        splineAnimate.enabled = true;
        splineAnimate.Play();
    }

    private void SetRenderersAndCollidersEnabled(bool enabled)
    {
        if (_renderers != null)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                    continue;

                _renderers[i].enabled = enabled && _rendererWasEnabled[i];
            }
        }

        if (_colliders != null)
        {
            for (int i = 0; i < _colliders.Length; i++)
            {
                if (_colliders[i] == null)
                    continue;

                _colliders[i].enabled = enabled && _colliderWasEnabled[i];
            }
        }
    }

    private void OnEnable()
    {
        _nextCullCheckTime = 0f;

        if (splineAnimate != null)
        {
            splineAnimate.Play();
        }
    }
}
