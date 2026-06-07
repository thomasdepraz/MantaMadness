using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using FMODUnity;

public enum CoinHolderType
{
    Challenge,
    Shop,
    AreaClear,
}
public class CoinHolder : MonoBehaviour, IDataPersistence
{
    public string coinName = "null";
    [SerializeField] private GameObject coin;
    [SerializeField] private ParticleSystem spawnParticle;
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineBlendDefinition blend;
    [SerializeField] private bool standaloneCoin;
    [SerializeField] private EventReference ClearSound;
    [SerializeField] private CoinHolderType type = CoinHolderType.Challenge;

    public bool hasBeenObtained;

    private void CheckActivation(bool value)
    {
        hasBeenObtained = value;
    }
    public void OnEnable()
    {
        coin.GetComponent<Coin>().pickedUpCoin += CheckActivation;
    }

    public void OnDisable()
    {
        coin.GetComponent<Coin>().pickedUpCoin -= CheckActivation;
    }

    public void LoadData(GameData data)
    {
        data.coinsCollected.TryGetValue(coinName, out hasBeenObtained);
        
        if (coin.activeSelf == true && standaloneCoin == false)
        {
            coin.SetActive(false);
            //greyCoin.SetActive(false);
            vcam.enabled = false;
        }
        else if (standaloneCoin == true)
        {
            if (hasBeenObtained == false)
            {
                coin.SetActive(true);
                //greyCoin.SetActive(false);
                vcam.enabled = false;
            }
            else
            {
                coin.SetActive(false);
                //greyCoin.SetActive(true);
                vcam.enabled = false;
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.coinsCollected.ContainsKey(coinName))
        {
            data.coinsCollected.Remove(coinName);
        }
        data.coinsCollected.Add(coinName, hasBeenObtained);
    }

    public void startProcess()
    {
        if(standaloneCoin == false)
        StartCoroutine(spawnCoinProcess(Game.Instance.player));
    }

    private IEnumerator spawnCoinProcess(SimpleController controller)
    {
        // PART 1 LOCK PLAYER ACTIVATE CAM
        //lock player
        controller.ForceLock(true);
        controller.RailLock(true);

        //activate camera + play sound
        vcam.enabled = true;
        CameraManager.Instance.BlendToCamera(vcam, blend);

        yield return new WaitForSeconds(2f);

        //PART 2 SPAWN IN SUN
        RuntimeManager.PlayOneShot(ClearSound, vcam.transform.position);
        spawnParticle.Play();

        //Particle depends on Type
        switch (type)
        {
            case CoinHolderType.Challenge:
                UIEffectManager.Instance.SpecificAction?.Invoke(UiParticles.CHALLENGE, "Armature_Chad");
                break;
            case CoinHolderType.Shop:
                UIEffectManager.Instance.SpecificAction?.Invoke(UiParticles.SHOPSUN, "Armature_Chad");
                break;
            case CoinHolderType.AreaClear:
                UIEffectManager.Instance.SpecificAction?.Invoke(UiParticles.AREACLEAR, "Armature_Chad");
                break;
        }

        if (hasBeenObtained == false)
        {
            //coin.transform.localScale = Vector3.zero;
            coin.SetActive(true);
        }
        else
        {
            //greyCoin.transform.localScale = Vector3.zero;
            //greyCoin.SetActive(true);
        }

        yield return new WaitForSeconds(3f);
        //PART 3 RESET TO DEFAULT

        //unlock player
        controller.ForceLock(false);
        controller.RailLock(false);


        //reset camera
        CameraManager.Instance.ResetCamera(vcam);
        vcam.enabled = false;
    }

    public void ForceSpawn()
    {
        if (hasBeenObtained)
            return;

        coin.SetActive(true);
    }
}
