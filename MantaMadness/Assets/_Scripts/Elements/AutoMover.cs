using UnityEngine;
using DG.Tweening;
using System;
using System.Collections;

[ExecuteInEditMode]
public class AutoMover : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float raycastDistance = 10f;
    public LayerMask detectionMask;
    public Vector3 rayOffset = Vector3.zero;
    private Vector3 origin;

#if UNITY_EDITOR
    private void Update()
    {
        if (Application.isPlaying == false)
        {
            origin = gameObject.transform.position;
            //transform.InverseTransformDirection(Vector3.down);

            if (Physics.Raycast(origin, -transform.up, out RaycastHit hit, raycastDistance, detectionMask))
            {
                gameObject.transform.position = hit.point + rayOffset;
                print("ALLO FILS DE PUTE");
            }
        }
    }
#endif
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(origin, origin + (-transform.up * raycastDistance));
    }
#endif
}
