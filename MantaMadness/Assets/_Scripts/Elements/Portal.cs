using UnityEngine;
using TMPro;
using FMODUnity;

[RequireComponent(typeof(BoxCollider))]
public class Portal : WorldCheckpoint
{
    [SerializeField] private string targetIndex;

    public Transform teleportPoint;
    private SimpleController player;

    [SerializeField] public TextMeshProUGUI signText;

    [Header("World State Parameters")]
    [SerializeField] private bool enterSecretRoom = false;

    [SerializeField] private MUSICS musicToPlay = MUSICS.NULL;
    [SerializeField] public WeatherType specialWeatherType = WeatherType.MountainTemple;

    [SerializeField] public GameObject[] levelToLoad;
    [SerializeField] public GameObject[] levelToUnload;

    [Header("Collectible Area")]
    [SerializeField]
    private string collectibleAreaID;



    protected override void Start()
    {
        base.Start();

        if(player == null)
        {
            player = Game.Instance.player;
        }

        if (signText != null)
        {
            signText.text = indexName;
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

        if (!string.IsNullOrEmpty(collectibleAreaID))
        {
            CollectibleAreaRegistry.Instance.SetCurrentArea(collectibleAreaID);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            WorldCheckpointManager.Instance.SetCheckpoint(respawnTransform, indexName, displayAreaName, nameToDisplay);
            PortalManager.Instance.StartCoroutine(PortalManager.Instance.Teleport(targetIndex, enterSecretRoom, musicToPlay,this, specialWeatherType));
            PortalManager.Instance.SetCheckpoint(targetIndex, displayAreaName, nameToDisplay);
        }
    }

    public override void EnableMat()
    {
        //RIEN
    }

    public override void DisableMat()
    {
        //RIEN
    }

}
