using UnityEngine;
using System.Collections;

public class ActivatorArea : MonoBehaviour
{
    [SerializeField] private GameObject[] activationList;
    [SerializeField] private float activationRadius = 20f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float checkInterval = 0.5f;

    private bool isActive;

    private void Start()
    {
        SetObjectsActive(false);

        StartCoroutine(CheckPlayerPresence());
    }

    private IEnumerator CheckPlayerPresence()
    {
        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            bool playerInside = Physics.CheckSphere(
                transform.position,
                activationRadius,
                playerLayer
            );

            if (playerInside != isActive)
            {
                isActive = playerInside;
                SetObjectsActive(isActive);
            }

            yield return wait;
        }
    }

    private void SetObjectsActive(bool active)
    {
        foreach (GameObject obj in activationList)
        {
            if (obj != null)
                obj.SetActive(active);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);
    }
#endif
}
