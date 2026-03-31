using Unity.Cinemachine;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class CameraZone : MonoBehaviour
{
    public static CameraZone ActiveZone {get; private set;} 

    public CinemachineCamera zoneCamera;

    [Header("Priorités Cinemachine")]
    public int activePriority = 20;
    public int inactivePriority = 5;

    [Header("Priorité logique de la zone")]
    public int zonePriority = 0;

    private Collider zoneCollider;
    private Transform player;

    private static readonly List<CameraZone> allZones = new();

    private void Awake()
    {
        zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;

        if (!allZones.Contains(this))
            allZones.Add(this);
    }

    private void OnDestroy()
    {
        allZones.Remove(this);
    }

    private void Start()
    {
        zoneCamera.Priority = inactivePriority;

        SimpleController p = Game.Instance.player;
        if (p != null)
            player = p.transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.GetComponent<SimpleController>())
            return;

        RefreshAllZones();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.GetComponent<SimpleController>())
            return;

        RefreshAllZones();
    }

    private void Update()
    {
        if (player == null)
        {
            SimpleController p = Game.Instance.player;
            if (p != null)
                player = p.transform;
            else
                return;
        }

        // Important pour gérer les téléportations
        RefreshAllZones();
    }

    private bool ContainsPlayer()
    {
        if (player == null)
            return false;

        Vector3 closest = zoneCollider.ClosestPoint(player.position);

        // Si le joueur est dans le collider, ClosestPoint renvoie sa position
        return Vector3.SqrMagnitude(closest - player.position) < 0.0001f;
    }

    private static void RefreshAllZones()
    {
        CameraZone bestZone = null;

        for (int i = 0; i < allZones.Count; i++)
        {
            CameraZone zone = allZones[i];

            if (zone == null || zone.player == null || zone.zoneCamera == null)
                continue;

            if (!zone.ContainsPlayer())
                continue;

            if (bestZone == null || zone.zonePriority > bestZone.zonePriority)
                bestZone = zone;
        }

        for (int i = 0; i < allZones.Count; i++)
        {
            CameraZone zone = allZones[i];

            if (zone == null || zone.zoneCamera == null)
                continue;

            zone.zoneCamera.Priority = (zone == bestZone)
                ? zone.activePriority
                : zone.inactivePriority;
        }

        ActiveZone = bestZone;
    }
}