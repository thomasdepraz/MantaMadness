using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameInterface : MonoBehaviour, IScreen
{
    public GameObject Container => m_Container;
    public GameObject m_Container;

    public Camera uiCamera;
    public TextMeshProUGUI coinText;
    public Image sunImage;

    public void Start()
    {
        CameraManager.Instance.AddCameraToStack(uiCamera);
        CoinManager.Instance.coinPickedUp += UpdateCoinCount;
        coinText.text = CoinManager.Instance.PickupCoinCount.ToString();
    }

    private void OnDestroy()
    {
        CoinManager.Instance.coinPickedUp -= UpdateCoinCount;
    }

    private void UpdateCoinCount(int coinCount)
    {
        coinText.text = coinCount.ToString();
        sunImage?.transform.DOPunchScale(Vector3.one, 1, 5);
    }
}
