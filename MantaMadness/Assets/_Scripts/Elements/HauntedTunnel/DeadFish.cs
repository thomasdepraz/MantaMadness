using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine;

public class DeadFish : MonoBehaviour
{
    private SimpleController player;
    public HauntedTunnel tunnel;
    private float speedMultiplier = 0.001f;

    [SerializeField] private Vector3 offset;

    private EventInstance instance;

    private void OnEnable()
    {
        StartCoroutine(DelayEnable());
    }


    private IEnumerator DelayEnable()
    {
        yield return new WaitForSeconds(0.1f);
        player = Game.Instance.player;
    }

    private void Update()
    {
        if (player == null)
            return;
        speedMultiplier += Time.deltaTime;
        MoveToTarget();
        CheckDistanceToTarget();
    }

    private void MoveToTarget()
    {
        float step = 1 * speedMultiplier * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, player.transform.position + offset, step);
    }

    private void CheckDistanceToTarget()
    {
        if (Vector3.Distance(transform.position, player.transform.position + offset) < 0.1f)
        {
            tunnel.SpawnAnomaly();
            Destroy(gameObject);
        }
    }
}
