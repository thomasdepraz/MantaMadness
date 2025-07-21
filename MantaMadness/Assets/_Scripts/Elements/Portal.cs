using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

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
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            PortalManager.Instance.StartCoroutine("Teleport", targetIndex);
        }
    }

}
