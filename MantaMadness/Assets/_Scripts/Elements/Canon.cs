using Unity.Cinemachine;
using UnityEngine;
using System.Collections;
using DG.Tweening;

public enum CanonType
{
    Shooter,
    Teleport,
}

public class Canon : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] protected ParticleSystem burstParticle;
    [SerializeField] private GameObject canonMain;
    [SerializeField] private Vector3 canonMainChargeScale;
    [SerializeField] private Vector3 canonMainShootScale;
    private Vector3 canonMainBaseScale;



    [Header("Parameters")]
    [SerializeField] protected float propulsionForce;
    [SerializeField] private Transform target;
    [SerializeField] private CanonType canonType;

    [Header("Teleport Parameters")]
    [SerializeField] protected string targetIndex;
    [SerializeField] protected bool enterSecretRoom = false;
    [SerializeField] protected MUSICS musicToPlay = MUSICS.NULL;
    [SerializeField] public WeatherType specialWeatherType = WeatherType.MountainTemple;


    [Header("Camera")]
    [SerializeField] protected CinemachineCamera targetCam;

    private Coroutine launchRoutine;

    protected void Start()
    {
        targetCam.enabled = false;
        canonMainBaseScale = canonMain.transform.localScale;
    }

    protected IEnumerator LaunchCoroutine(SimpleController player)
    {
        player.StopByTargetImpact(target.gameObject);
        targetCam.enabled = true;
        canonMain.transform.DOScale(canonMainChargeScale, 1.2f).SetEase(Ease.InBounce);
        //LockPlayer + make invisible

        //Play sound enter canon
        //Tween canon CHARGE UP
        //Play particle
        yield return new WaitForSeconds(1.2f);

        if(canonType == CanonType.Shooter)
        {
            canonMain.transform.DOScale(canonMainShootScale, 0.25f).SetEase(Ease.OutBounce);
            player.togglePlayerBodyVisual(true);
            player.PropelledByTarget(target, propulsionForce);
            burstParticle.Play();
            yield return new WaitForSeconds(0.25f);
            canonMain.transform.DOScale(canonMainBaseScale, 0.55f).SetEase(Ease.InBounce);
            targetCam.enabled = false;
        }
        else if (canonType == CanonType.Teleport)
        {
            canonMain.transform.DOScale(canonMainShootScale, 0.25f).SetEase(Ease.OutBounce);
            burstParticle.Play();
            yield return new WaitForSeconds(0.55f);
            targetCam.enabled = false;
            player.togglePlayerBodyVisual(true);
            PortalManager.Instance.StartCoroutine(PortalManager.Instance.Teleport(targetIndex, enterSecretRoom, musicToPlay, specialWeatherType));
            yield return new WaitForSeconds(0.25f);
            canonMain.transform.DOScale(canonMainBaseScale, 0.55f).SetEase(Ease.InBounce);
        }
        launchRoutine = null;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SimpleController>() != null)
        {
            if(launchRoutine == null)
            {
                launchRoutine = StartCoroutine(LaunchCoroutine(other.GetComponent<SimpleController>()));
            }
        }
    }
}
