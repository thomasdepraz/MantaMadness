using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    public string index;
    public string targetIndex;

    public Transform teleportPoint;
    private SimpleController player;

    private void Start()
    {
        if(player == null)
        {
            player = Game.Instance.player;
        }
    }

    public void Teleport()
    {
        player.transform.position = teleportPoint.position;
        player.transform.forward = teleportPoint.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            PortalManager.Instance.StartCoroutine("Teleport", targetIndex);
        }
    }

}
