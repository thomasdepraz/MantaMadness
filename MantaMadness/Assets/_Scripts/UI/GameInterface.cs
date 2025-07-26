using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameInterface : MonoBehaviour, IScreen
{
    public GameObject Container => m_Container;
    public GameObject m_Container;

    public Camera uiCamera;
    public TextMeshProUGUI coinText;
    public Image sunImage;
    public Image sunOverlay;

    public void Start()
    {
        UIManager.Instance.gameInterface = this;
        CameraManager.Instance.AddCameraToStack(uiCamera);
        CoinManager.Instance.coinPickedUp += UpdateCoinCount;
        coinText.text = CoinManager.Instance.PickupCoinCount.ToString();
        if(sunOverlay.IsActive() == true)
        {
            sunOverlay.enabled = false;
        }
    }

    private void OnDestroy()
    {
        CoinManager.Instance.coinPickedUp -= UpdateCoinCount;
    }

    public void UpdateCoinCount(int coinCount)
    {
        coinText.text = coinCount.ToString();
        sunImage?.transform.DOPunchScale(Vector3.one, 1, 5);
    }
    public void toggleSunOverlay(bool toggleValue)
    {
        if (toggleValue == false)
        {
            sunOverlay.enabled = false;
        }
        else
        {
            sunOverlay.enabled = true;
        }
    }

    public IEnumerator pickupMegaClam()
    {
        toggleSunOverlay(true);
        UIEffectManager.Instance.SpecificAction?.Invoke("MEGACLAM", "Armature_megaClam");
        yield return new WaitForSeconds(2f);
        toggleSunOverlay(false);
    }

    public void pickupJohnnyParticle()
    {
        UIEffectManager.Instance.SpecificAction?.Invoke("JOHNNY", "");
    }
}
