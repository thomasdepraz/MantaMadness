using System.Drawing;
using UnityEngine;
using System.Collections;

public class AlligatorArea : MonoBehaviour
{
    public GameObject alligatorPrefab;
    public SimpleController player;

    [SerializeField] private Transform center;
    [SerializeField] private Vector3 size;
    [SerializeField] private LayerMask playerLayer;

    private Coroutine checkCoroutine;

    [Header("Settings")]
    [SerializeField] private float checkInterval = 0.2f;

    public float minSpawnRadius = 10f;
    public float maxSpawnRadius = 20f;
    public int beatInterval = 4; // spawn tous les 4 beats

    private bool playerInside = false;
    private int beatCounter = 0;

    public LayerMask terrainLayer;

    private void Start()
    {
        player = Game.Instance.player;
        MusicManager.OnBeat += HandleBeat;
    }
    private void OnEnable()
    {
        checkCoroutine = StartCoroutine(CheckTunnelRoutine());
    }

    private void OnDisable()
    {
        MusicManager.OnBeat -= HandleBeat;
    }

    private void HandleBeat(int bar, int beat, float tempo)
    {
        if (!playerInside) return;

        beatCounter++;

        if (beatCounter % beatInterval == 0)
        {
            SpawnAlligator();
        }
    }

    private void SpawnAlligator()
    {
        Debug.Log("JE Start le spawn du croco");
        for (int i = 0; i < 10; i++) // 10 tentatives max
        {
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(minSpawnRadius, maxSpawnRadius);
            Vector3 randomPos = player.transform.position + new Vector3(circle.x, 0, circle.y);
            randomPos.y += 20f;

            RaycastHit hit;

            if (Physics.Raycast(randomPos, Vector3.down, out hit, 50f, terrainLayer))
            {
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Water"))
                    continue;

                Vector3 spawnPos = hit.point;

                if (!IsInCameraView(spawnPos))
                    continue;

                GameObject croco = Instantiate(alligatorPrefab, spawnPos, Quaternion.identity);

                Vector3 direction = (player.transform.position - spawnPos);
                direction.y = 0;
                direction.Normalize();

                croco.GetComponent<AlligatorBehavior>().Init(direction);

                return;
            }
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        playerInside = true;
    //        beatCounter = 0; // reset optionnel
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        playerInside = false;
    //    }
    //}

    private bool IsInCameraView(Vector3 position)
    {
        Vector3 camForward = CameraTargetDetection.Instance.GetDetectionForward();
        Vector3 origin = CameraTargetDetection.Instance.GetDetectionOrigin();

        Vector3 dirToSpawn = (position - origin).normalized;

        float angle = Vector3.Angle(camForward, dirToSpawn);
        float fov = CameraTargetDetection.Instance.GetCurrentViewAngle();

        return angle < fov * 0.5f;
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
        bool isInZone = Physics.CheckBox(center.position, size / 2f, center.rotation, playerLayer);

        if (isInZone == playerInside)
            return;


        Debug.Log("Player is in zone = " + isInZone);
        playerInside = isInZone;
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
