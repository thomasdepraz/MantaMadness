using UnityEngine;
using TMPro;

[RequireComponent(typeof(BoxCollider))]
public class Portal : WorldCheckpoint
{
    [SerializeField] protected string targetIndex;

    public Transform teleportPoint;
    protected SimpleController player;

    [SerializeField] public TextMeshProUGUI signText;

    [Header("World State Parameters")]
    [SerializeField] protected bool enterSecretRoom = false;

    [SerializeField] protected MUSICS musicToPlay = MUSICS.NULL;
    [SerializeField] public WeatherType specialWeatherType = WeatherType.MountainTemple;

    [SerializeField] protected AreaIntro areaIntro;

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

    public virtual void Teleport()
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
            //WorldCheckpointManager.Instance.SetCheckpoint(respawnTransform, indexName, displayAreaName, nameToDisplay, LevelID, collectibleAreaID);
            PortalManager.Instance.SetCheckpoint(targetIndex, displayAreaName, nameToDisplay, LevelID, collectibleAreaID);
            if(areaIntro !=  null)
            {
                PortalManager.Instance.StartCoroutine(PortalManager.Instance.Teleport(targetIndex, enterSecretRoom, musicToPlay, specialWeatherType, areaIntro, this));
            }
            else
            {
                PortalManager.Instance.StartCoroutine(PortalManager.Instance.Teleport(targetIndex, enterSecretRoom, musicToPlay, specialWeatherType,null, this));
            }
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
