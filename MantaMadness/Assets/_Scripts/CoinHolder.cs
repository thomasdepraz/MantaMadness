using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using FMODUnity;

public class CoinHolder : MonoBehaviour
{
    public string coinName = "null";
    [SerializeField] private GameObject coin;
    [SerializeField] private ParticleSystem spawnParticle;
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineBlendDefinition blend;
    [SerializeField] private bool standaloneCoin;
    [SerializeField] private EventReference ClearSound;

    private bool hasBeenObtained = false;

    //public Action<>

    public void Start()
    {
        if(coin.activeSelf == true && standaloneCoin == false)
        {
            coin.SetActive(false);
            vcam.enabled = false;
        }
        else if (standaloneCoin == true)
        {
            coin.SetActive(true);
        }
    }

    public void startProcess()
    {
        if(standaloneCoin == false && hasBeenObtained == false)
        StartCoroutine(spawnCoinProcess(Game.Instance.player));
    }
    private IEnumerator spawnCoinProcess(SimpleController controller)
    {
        // PART 1 LOCK PLAYER ACTIVATE CAM
        //lock player
        controller.ForceLock(true);

        //activate camera + play sound
        vcam.enabled = true;
        CameraManager.Instance.BlendToCamera(vcam, blend);

        yield return new WaitForSeconds(2f);

        //PART 2 SPAWN IN SUN
        RuntimeManager.PlayOneShot(ClearSound, vcam.transform.position);
        spawnParticle.Play();
        UIEffectManager.Instance.SpecificAction?.Invoke("CHALLENGE", "Armature_Chad");
        coin.transform.localScale = Vector3.zero;
        coin.SetActive(true);
        yield return new WaitForSeconds(3f);
        //PART 3 RESET TO DEFAULT

        //unlock player
        controller.ForceLock(false);
    

        //reset camera
        CameraManager.Instance.ResetCamera(vcam);
        vcam.enabled = false;
        hasBeenObtained = true;
    }
}
