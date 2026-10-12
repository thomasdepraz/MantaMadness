using System.Drawing;
using UnityEngine;
using System.Collections;

public class CarsManager : MonoBehaviour
{
    bool wasInTunnel;

    [SerializeField] private Transform center;
    [SerializeField] private Vector3 size;
    [SerializeField] private LayerMask playerLayer;

    [Header("Settings")]
    [SerializeField] private float checkInterval = 0.2f;

    private Coroutine checkCoroutine;

    [SerializeField] private CarsSplineAnimate[] cars;

    private void OnEnable()
    {
        ToggleAllCars(false);
        checkCoroutine = StartCoroutine(CheckTunnelRoutine());
    }
    private void OnDisable()
    {
        if (checkCoroutine != null)
        {
            StopCoroutine(checkCoroutine);
            checkCoroutine = null;
        }
    }

    private IEnumerator CheckTunnelRoutine()
    {
        CheckTunnel();

        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            yield return wait;
            CheckTunnel();
        }
    }

    public void CheckTunnel()
    {
        bool isInTunnel = Physics.CheckBox(center.position, size / 2f, center.rotation, playerLayer);

        if (isInTunnel == wasInTunnel)
            return;

        wasInTunnel = isInTunnel;

        if (isInTunnel)
        {
            // ENTER TUNNEL => DEACTIVATE MUSIC + UI

            ToggleAllCars(true);
        }
        else
        {
            //REACTIVATE MUSIC + UI
            ToggleAllCars(false);
        }
    }

    private void ToggleAllCars(bool toggle)
    {
        foreach(CarsSplineAnimate car in cars)
        {
            car.gameObject.SetActive(toggle);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (center == null)
            return;

        Gizmos.matrix = Matrix4x4.TRS(
            center.position,
            center.rotation,
            Vector3.one
        );

        Gizmos.DrawWireCube(Vector3.zero, size);
    }
#endif
}
