using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    [SerializeField] public string index;
    [SerializeField] private string targetIndex;

    public Transform teleportPoint;
    private SimpleController player;

    public GameObject objectTest;

    [SerializeField] public TextMeshProUGUI signText;

    private void Start()
    {
        if(player == null)
        {
            player = Game.Instance.player;
        }

        if (objectTest == null)
        {
            Debug.LogError("signText is not assigned in the inspector!", this);
            return;
        }

        if (signText == null)
        {
            Debug.LogError("signText is not assigned in the inspector!", this);
            return;
        }

        signText.text = index;

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
