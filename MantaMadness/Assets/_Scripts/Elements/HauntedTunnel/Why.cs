using FMODUnity;
using System.Collections;
using System.Drawing;
using UnityEngine;

public class Why : MonoBehaviour
{
    [SerializeField] private float checkInterval;
    [SerializeField] private Transform center;
    [SerializeField] private Vector3 size;
    [SerializeField] private LayerMask playerLayer;
    public EventReference laugh;


    private void OnEnable()
    {
        StartCoroutine(CheckRoutine());
    }
    private IEnumerator CheckRoutine()
    {

        Check();

        WaitForSeconds wait = new WaitForSeconds(checkInterval);

        while (true)
        {
            yield return wait;
            Check();
        }
    }

    public void Check()
    {
        bool isInTunnel = Physics.CheckBox(center.position, size / 2f, center.rotation, playerLayer);

        if (isInTunnel)
        {
            if(endRoutine == null)
            {
                endRoutine = StartCoroutine(EndCoroutine());
            }
        }

    }

    private Coroutine endRoutine;

    private IEnumerator EndCoroutine()
    {
        RuntimeManager.PlayOneShot(laugh, Camera.main.transform.position);
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(1.5f);
        Application.Quit();
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
