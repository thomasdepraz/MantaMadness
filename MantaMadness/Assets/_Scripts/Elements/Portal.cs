using UnityEngine;
using TMPro;

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

    [Header("Collectible Area")]
    [SerializeField]
    public string collectibleAreaID;


    protected override void Start()
    {
        base.Start();

        if (player == null)
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

        if (CameraTargetController.instance != null)
            CameraTargetController.instance.SyncYawPitchToPlayerFacing();

        //if (!string.IsNullOrEmpty(collectibleAreaID))
        //{
        //    CollectibleAreaRegistry.Instance.SetCurrentArea(collectibleAreaID);
        //}
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            WorldCheckpointManager.Instance.SetCheckpoint(respawnTransform, indexName, displayAreaName, nameToDisplay, LevelID);
            PortalManager.Instance.SetCheckpoint(targetIndex, displayAreaName, nameToDisplay, LevelID);
            PortalManager.Instance.StartCoroutine(PortalManager.Instance.Teleport(targetIndex, enterSecretRoom, musicToPlay,this, specialWeatherType));
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
