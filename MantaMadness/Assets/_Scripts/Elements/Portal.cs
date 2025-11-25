using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class Portal : MonoBehaviour
{
    [SerializeField] public string index;
    [SerializeField] private string targetIndex;

    public Transform teleportPoint;
    private SimpleController player;


    [SerializeField] public TextMeshProUGUI signText;
    [Header("Checkpoint Parameters")]
    [SerializeField] public bool displayAreaName = false;
    [SerializeField] public string nameToDisplay;

    [SerializeField] private bool enterSecretRoom = false;

    private void Start()
    {
        if(player == null)
        {
            player = Game.Instance.player;
        }

        if (signText != null)
        {
            signText.text = index;
        }

        if (!PortalManager.Instance.portals.Contains(this))
        {
            PortalManager.Instance.portals.Add(this);
        }
    }

    public void Teleport()
    {
        player.transform.position = teleportPoint.position;
        player.transform.rotation = new Quaternion(0, teleportPoint.transform.rotation.y, 0, teleportPoint.transform.rotation.w);
        //player.transform.forward = teleportPoint.forward;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            PortalManager.Instance.StartCoroutine(PortalManager.Instance.Teleport(targetIndex, enterSecretRoom));
            PortalManager.Instance.SetCheckpoint(targetIndex, displayAreaName, nameToDisplay);
        }
    }

}
