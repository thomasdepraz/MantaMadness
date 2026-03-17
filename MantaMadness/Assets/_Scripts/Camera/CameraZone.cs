using Unity.Cinemachine;
using UnityEngine;

public class CameraZone : MonoBehaviour
{
    public CinemachineCamera zoneCamera;
    public int activePriority = 20;
    public int inactivePriority = 5;

    private void Start()
    {
        zoneCamera.Priority = inactivePriority;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            zoneCamera.Priority = activePriority;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            zoneCamera.Priority = inactivePriority;
        }
    }
}