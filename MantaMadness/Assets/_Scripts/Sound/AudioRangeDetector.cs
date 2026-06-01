using System.Collections.Generic;
using UnityEngine;

public class AudioRangeDetector : MonoBehaviour
{
    [SerializeField] private float radius = 30f;
    [SerializeField] private LayerMask audioLayer;
    [SerializeField] private float refreshRate = 0.25f;

    private readonly HashSet<IAudioCullable> activeCullables = new();

    private void Start()
    {
        InvokeRepeating(nameof(CheckRange), 0f, refreshRate);
    }

    private void CheckRange()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            radius,
            audioLayer);

        HashSet<IAudioCullable> currentCullables = new();

        foreach (var hit in hits)
        {
            IAudioCullable cullable =
                hit.GetComponentInParent<IAudioCullable>();

            if (cullable == null)
                continue;

            currentCullables.Add(cullable);

            if (!activeCullables.Contains(cullable))
            {
                cullable.OnAudioRangeEnter();
            }
        }

        foreach (var cullable in activeCullables)
        {
            if (!currentCullables.Contains(cullable))
            {
                cullable.OnAudioRangeExit();
            }
        }

        activeCullables.Clear();

        foreach (var cullable in currentCullables)
        {
            activeCullables.Add(cullable);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
