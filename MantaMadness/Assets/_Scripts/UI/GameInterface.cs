using DG.Tweening;
using EasyTextEffects;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameInterface : MonoBehaviour, IScreen
{
    public GameObject Container => m_Container;
    public GameObject m_Container;

    public Camera uiCamera;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI clamText;
    public Image sunImage;
    public Image sunOverlay;

    [Header("Area Name Parameters")]
    [SerializeField] private RectTransform startPosition;
    [SerializeField] private RectTransform endPosition;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextEffect textEffects;

    [Header("Black Bar Effect Parameters")]
    [SerializeField] private GameObject topBar;
    [SerializeField] private GameObject bottomBar;
    [SerializeField] private RectTransform topBarStartPosition;
    [SerializeField] private RectTransform bottomBarStartPosition;
    [SerializeField] private Vector3 barOffset;


    public void Start()
    {
        UIManager.Instance.gameInterface = this;
        CameraManager.Instance.AddCameraToStack(uiCamera);
        CoinManager.Instance.coinPickedUp += UpdateCoinCount;
        CoinManager.Instance.collectiblePickedUp += UpdateCollectibleCount;
        coinText.text = CoinManager.Instance.PickupCoinCount.ToString();
        textEffects.StartManualEffects();
        text.enabled = false;
        if(sunOverlay.IsActive() == true)
        {
            sunOverlay.enabled = false;
        }
    }

    private void OnDestroy()
    {
        CoinManager.Instance.coinPickedUp -= UpdateCoinCount;
        CoinManager.Instance.collectiblePickedUp -= UpdateCollectibleCount;
    }

    public void UpdateCoinCount(int coinCount)
    {
        coinText.text = coinCount.ToString();
        sunImage?.transform.DOPunchScale(Vector3.one, 1, 5);
    }

    public void UpdateCollectibleCount(int collectibleCount)
    {
        clamText.text = collectibleCount.ToString();
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
    public void StartDisplayCoroutine(string name)
    {
        StartCoroutine(DisplayCoroutine(name));
    }
    private IEnumerator DisplayCoroutine(string name)
    {
        if(text.enabled == false)
        {
            text.enabled = true;
        }
        text.DOKill();
        text.text = name;
        textEffects.StartManualEffects();
        text.transform.localScale = Vector3.one;
        text.transform.position = startPosition.position;
        text.transform.DOMove(endPosition.position, 1.5f).SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(4f);
        text.transform.DOMove(startPosition.position, 1.5f).SetEase(Ease.InQuad);
    }

    public void ToggleBlackBarEffect(bool enable, float duration)
    {
        if (enable && blackBarEffectRoutine == null)
        {
            blackBarEffectRoutine = StartCoroutine(EnableBlackBarEffect(duration));
        }
        else if(enable == false && blackBarEffectRoutine == null)
        {
            blackBarEffectRoutine = StartCoroutine(DisableBlackBarEffect(duration));
        }
    }
    private Coroutine blackBarEffectRoutine;
    private IEnumerator EnableBlackBarEffect(float duration)
    {
        topBar.transform.DOMove(topBarStartPosition.transform.position - barOffset, duration / 2f).SetEase(Ease.OutQuad);
        bottomBar.transform.DOMove(bottomBarStartPosition.position + barOffset, duration / 2f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(duration);
        blackBarEffectRoutine = null;
    }

    private IEnumerator DisableBlackBarEffect(float duration)
    {
        topBar.transform.DOMove(topBarStartPosition.transform.position, duration / 2f).SetEase(Ease.OutQuad);
        bottomBar.transform.DOMove(bottomBarStartPosition.position, duration / 2f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(duration);
        blackBarEffectRoutine = null;
    }
    public void ToggleInterface(bool toggle)
    {
        m_Container.SetActive(toggle);
    }
}
