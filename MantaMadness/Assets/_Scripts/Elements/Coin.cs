using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Coin : MonoBehaviour, ISaveable
{
    const float c_LockDuration = 3f;

    [Header("Cinemachine")]
    public CinemachineCamera vcamera;
    public CinemachineBlendDefinition blend;

    [Header("Saving")]
    public string saveName;
    public bool isMiniGameCoin;
    bool ISaveable.CanSave => !isMiniGameCoin;

    private bool pickedUp = false;
    private WaitForSeconds wait;
    private Coroutine routine;


    private void Start()
    {
        if (isMiniGameCoin)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(!pickedUp);
        }
    }

    private IEnumerator PickupCoroutine(SimpleController controller)
    {
        wait = new WaitForSeconds(c_LockDuration);

        //lock player
        controller.ForceLock(true);

        //activate camera
        CameraManager.Instance.BlendToCamera(vcamera, blend);

        // Sound
        SoundManager.Instance.PlayOneShotSound(SoundType.COINPICKUP);

        //increase boost gauge
        Game.Instance.player.boostBehaviour.IncrementGauge(BoostAction.CoolSun);

        //animation

        //
        UIManager.Instance.gameInterface.playPickupParticle(1);

        yield return wait;

        //unlock player
        controller.ForceLock(false);

        //reset camera
        CameraManager.Instance.ResetCamera(vcamera);

        //increase coin count
        CoinManager.Instance.PickupCoin();

        //Deactivate game object
        routine = null;

        gameObject.SetActive(false);
        pickedUp = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out SimpleController controller) && routine == null)
        {
            routine = StartCoroutine(PickupCoroutine(controller));
        }
    }

    void ISaveable.Save()
    {
        PlayerPrefs.SetInt(Constants.c_CoinPrefixSave + saveName, pickedUp ? 1 : 0);
    }

    void ISaveable.Load()
    {
        int save = PlayerPrefs.GetInt(Constants.c_CoinPrefixSave + this.GetHashCode().ToString(), 0);
        pickedUp = save == 0 ? false : true;
        Start();
    }
}
