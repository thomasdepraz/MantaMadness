using Unity.Cinemachine;
using UnityEngine;


public class AirRail : MonoBehaviour
{
    private new BoxCollider collider;
    public Transform direction;
    public CinemachineCamera rideCamera;

    [Header("Parameters")]
    public float rideForce;
    public float rideDistance;

    private bool playerInCollider;
    private SimpleController controller;
    private void Awake()
    {
        collider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if(playerInCollider && controller != null)
        {
            controller.EnterAirRail(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent(out SimpleController controller))
        {
            controller.EnterAirRail(this);
            playerInCollider = true;
            this.controller = controller;
            enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.TryGetComponent(out SimpleController controller))
        {
            playerInCollider = false;
            this.controller = null;
            enabled = false;
        }
    }

    public bool InAirRail(Vector3 position)
    {
        if ((position - transform.position).sqrMagnitude > rideDistance * rideDistance)
            return false;

        return true;
    }

    private void OnDrawGizmos()
    {
        if (collider == null)
            collider = GetComponent<BoxCollider>();

        if (direction == null)
            return;

        Vector3 extents = collider.bounds.extents;

        Vector3 a = collider.center + new Vector3(extents.x, extents.y, extents.z);
        Vector3 b = collider.center + new Vector3(extents.x, -extents.y, extents.z);
        Vector3 c = collider.center + new Vector3(-extents.x, extents.y, extents.z);
        Vector3 d = collider.center + new Vector3(-extents.x, -extents.y, extents.z);

        Gizmos.DrawRay(transform.position + a, direction.forward * rideDistance);
        Gizmos.DrawRay(transform.position + b, direction.forward * rideDistance);
        Gizmos.DrawRay(transform.position + c, direction.forward * rideDistance);
        Gizmos.DrawRay(transform.position + d, direction.forward * rideDistance);
    }
}
