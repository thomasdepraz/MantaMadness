using UnityEngine;

public class AlligatorArea : MonoBehaviour
{
    public GameObject alligatorPrefab;
    public SimpleController player;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            beatCounter = 0; // reset optionnel
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private bool IsInCameraView(Vector3 position)
    {
        Vector3 camForward = CameraTargetDetection.Instance.GetDetectionForward();
        Vector3 origin = CameraTargetDetection.Instance.GetDetectionOrigin();

        Vector3 dirToSpawn = (position - origin).normalized;

        float angle = Vector3.Angle(camForward, dirToSpawn);
        float fov = CameraTargetDetection.Instance.GetCurrentViewAngle();

        return angle < fov * 0.5f;
    }
}
