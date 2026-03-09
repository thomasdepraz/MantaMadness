using System.Collections.Generic;
using UnityEngine;

public class FeverDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] float radius = 10f;
    [SerializeField] LayerMask feverLayer;

    [SerializeField] int maxColliders = 50;

    Collider[] hitBuffer;
    HashSet<FeverObject> currentObjects = new HashSet<FeverObject>();
    HashSet<FeverObject> detectedThisFrame = new HashSet<FeverObject>();

    void Awake()
    {
        hitBuffer = new Collider[maxColliders];
    }

    void Update()
    {
        if (ComboManager.Instance.State != ComboState.Fever)
        {
            if (currentObjects.Count > 0)
                Debug.Log("[FeverDetector] Fever ended → Reset objects");

            ResetObjects();
            return;
        }

        Debug.Log("[FeverDetector] Scanning for FeverObjects...");

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            radius,
            hitBuffer,
            feverLayer
        );

        Debug.Log($"[FeverDetector] Colliders detected: {hitCount}");

        detectedThisFrame.Clear();

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = hitBuffer[i];

            Debug.Log($"[FeverDetector] Collider found: {hit.name}");

            FeverObject obj = hit.GetComponent<FeverObject>();

            if (obj == null)
            {
                Debug.Log($"[FeverDetector] {hit.name} has no FeverObject");
                continue;
            }

            if (obj.Type != FeverObjectType.OnRange)
            {
                Debug.Log($"[FeverDetector] {hit.name} is not OnRange type");
                continue;
            }

            detectedThisFrame.Add(obj);

            if (!currentObjects.Contains(obj))
            {
                Debug.Log($"[FeverDetector] ENTER range → {obj.name}");

                currentObjects.Add(obj);
                obj.OnFeverRange();
            }
        }

        List<FeverObject> toRemove = new List<FeverObject>();

        foreach (var obj in currentObjects)
        {
            if (!detectedThisFrame.Contains(obj))
            {
                Debug.Log($"[FeverDetector] EXIT range → {obj.name}");

                obj.OnFeverReset();
                toRemove.Add(obj);
            }
        }

        foreach (var obj in toRemove)
        {
            currentObjects.Remove(obj);
        }
    }

    void ResetObjects()
    {
        foreach (var obj in currentObjects)
        {
            Debug.Log($"[FeverDetector] Reset object → {obj.name}");
            obj.OnFeverReset();
        }

        currentObjects.Clear();
        detectedThisFrame.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}