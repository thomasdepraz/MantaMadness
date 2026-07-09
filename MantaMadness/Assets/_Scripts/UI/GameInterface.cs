using DG.Tweening;
using EasyTextEffects;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameInterface : MonoBehaviour, IScreen
{
    public GameObject Container => m_Container;
    public GameObject m_Container;

    public Camera uiCamera;
    [Header("UI Ressources Parameters")]
    public GameObject coinUiVisual;
    public GameObject clamUiVisual;
    public GameObject buckieUiVisual;
    public TextMeshProUGUI sunText;
    public TextMeshProUGUI clamText;
    public TextMeshProUGUI buckieText;

    [Header("UI Current Collectible Area")]
    public TextMeshProUGUI collectibleAreaName;
    public TextMeshProUGUI clamAreaCount;
    public TextMeshProUGUI buckieAreaCount;
    public TextMeshProUGUI SunAreaCount;

    [Header("UI otal Collectible Menu")]
    public TextMeshProUGUI collectibleAreaNameMenu;
    public TextMeshProUGUI totalClamCountMenu;
    public TextMeshProUGUI totalBuckieCountMenu;
    public TextMeshProUGUI totalSunCountMenu;


    [Header("Sun Overlay Parameters")]
    public Image sunImage;
    public Image sunOverlay;
    public Image sunCountImage;
    [SerializeField] private Vector3[] sunScales =
    {
        Vector3.one * 0.5f,
        Vector3.one * 0.6f,
        Vector3.one * 0.7f,
        Vector3.one * 0.8f,
        Vector3.one, //FEVER
    };

    [Header("Speed Gauge")]
    public GameObject speedGaugeObject;

    [Header("Cat Ability Parameters")]
    public Image catOverlay;
    public VideoPlayer catVideoPlayer;
    public ParticleSystem[] catParticles;

    [Header("Area Name Parameters")]
    [SerializeField] private RectTransform startPosition;
    [SerializeField] private RectTransform endPosition;
    [SerializeField] private RectTransform textRect;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextEffect textEffects;

    [Header("Black Bar Effect Parameters")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;
    [SerializeField] private RectTransform topBarStartPosition;
    [SerializeField] private RectTransform bottomBarStartPosition;

    [Header("End Screen Parameters")]
    [SerializeField] private GameObject endScreenVisual;
    [SerializeField] private GameObject endScreenInteractText;
    private Vector3 endScreenOriginalScale;


    private InputActionMap playerActionsMap;
    private InputAction interactAction;

    private Vector2 GetBarOffset()
    {
        float offsetY = Screen.height * 0.1f;
        return new Vector2(0, offsetY);
    }
    private Vector2 GetScreenRelativePosition(float xPercent, float yPercent)
    {
        return new Vector2(
            (xPercent - 0.5f) * Screen.width,
            (yPercent - 0.5f) * Screen.height
        );
    }

    // FMOD beats still fire while paused; skip new tweens so they cannot stack on resume.
    private static bool GameplayPaused()
    {
        return Time.timeScale == 0f;
    }

    public void Start()
    {
        UIManager.Instance.gameInterface = this;
        CameraManager.Instance.AddCameraToStack(uiCamera);

        CoinManager.Instance.coinPickedUp += UpdateCoinCount;
        CoinManager.Instance.buckiePickedUp += UpdateBuckieCount;
        CoinManager.Instance.clamPickedUp += UpdateClamCount;
        //coinText.text = CoinManager.Instance.PickupCoinCount.ToString();
        totalSunCountMenu.text = CoinManager.Instance.PickupCoinCount.ToString();
        ComboManager.Instance.OnComboLevelChanged += SunComboBehavior;
        MusicManager.OnBeat += SunOnBeatFever;
        MusicManager.OnBeat2 += SunOnBeat;

        textRect = text.GetComponent<RectTransform>();
        textEffects.StartManualEffects();
        text.enabled = false;

        endScreenOriginalScale = endScreenVisual.transform.localScale;
        endScreenVisual.SetActive(false);
        endScreenInteractText.SetActive(false);


        if(sunOverlay.IsActive() == true)
        {
            sunOverlay.enabled = false;
        }

        if(catOverlay.IsActive() == true)
        {
            catOverlay.enabled = false;
        }

        sunImage.transform.localScale = sunScales[0];

        RefreshAllAreaCount();
    }

    private void OnEnable()
    {
        playerActionsMap = InputSystem.actions.FindActionMap("Player");
        interactAction = playerActionsMap.FindAction("Interact");
    }

    private void OnDestroy()
    {
        CoinManager.Instance.coinPickedUp -= UpdateCoinCount;
        CoinManager.Instance.buckiePickedUp -= UpdateBuckieCount;
        CoinManager.Instance.clamPickedUp -= UpdateClamCount;
        ComboManager.Instance.OnComboLevelChanged -= SunComboBehavior;
        MusicManager.OnBeat -= SunOnBeatFever;
        MusicManager.OnBeat2 -= SunOnBeat;
        Debug.Log("GameInterface destroyed");
    }

    public void UpdateCoinCount(int coinCount)
    {
        totalSunCountMenu.text = coinCount.ToString();
        //coinText.text = coinCount.ToString();
        if (GameplayPaused() == false && sunCountImage != null)
            sunCountImage?.transform.DOPunchScale(Vector3.one, 1, 5);
    }

    public void UpdateClamCount(int clamCount)
    {
        //clamText.text = clamCount.ToString();
        totalClamCountMenu.text = clamCount.ToString();
    }

    public void UpdateBuckieCount(int buckieCount)
    {
        //buckieText.text = buckieCount.ToString();
        totalBuckieCountMenu.text = buckieCount.ToString();
    }

    public void toggleSunInterface(bool toggleValue)
    {
        if (toggleValue == false)
        {
            sunImage.enabled = false;
        }
        else
        {
            sunImage.enabled = true;
        }
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

    public void playCatVideo()
    {
        StartCoroutine(CatVideoCoroutine());
    }

    public IEnumerator CatVideoCoroutine()
    {
        if (!catVideoPlayer.isPrepared)
            catVideoPlayer.Prepare();

        yield return new WaitUntil(() => catVideoPlayer.isPrepared);

        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CAT);

        catOverlay.enabled = true;

        //Get la duréé de la vid
        double duration = (double)catVideoPlayer.frameCount / catVideoPlayer.frameRate;
        catVideoPlayer.time = 0;
        catVideoPlayer.Play();

        foreach(ParticleSystem vfx in catParticles)
        {
            vfx.Play();
        }
        yield return new WaitForSeconds((float)duration);

        foreach (ParticleSystem vfx in catParticles)
        {
            vfx.Stop();
        }

        catVideoPlayer.Stop();
        catOverlay.enabled = false;
        Game.Instance.player.catRoutine = null;
    }

    public IEnumerator pickupMegaClam()
    {
        toggleSunOverlay(true);
        UIEffectManager.Instance.SpecificAction?.Invoke(UiParticles.MEGACLAM, "Armature_megaClam");
        yield return new WaitForSeconds(2f);
        toggleSunOverlay(false);
    }

    public void pickupJohnnyParticle()
    {
        UIEffectManager.Instance.SpecificAction?.Invoke(UiParticles.JOHNNYFOUND, "");
    }

    public void pickupSpecialItem(UiParticles name)
    {
        UIEffectManager.Instance.SpecificAction?.Invoke(name, "Armature_Chad");
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
        textRect.anchoredPosition = startPosition.anchoredPosition;

        textRect.DOAnchorPos(GetScreenRelativePosition(0.5f,0.85f), 1.5f).SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(4f);
        textRect.DOAnchorPos(GetScreenRelativePosition(0.5f, 1.2f), 1.5f).SetEase(Ease.InQuad);
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
        else if(enable && blackBarEffectRoutine != null)
        {
            ResetBlackBarEffect();
            blackBarEffectRoutine = null;
            blackBarEffectRoutine = StartCoroutine(EnableBlackBarEffect(duration));
        }
        else if (enable == false && blackBarEffectRoutine != null)
        {
            ResetBlackBarEffect();
            blackBarEffectRoutine = null;
            blackBarEffectRoutine = StartCoroutine(DisableBlackBarEffect(duration));
        }
    }
    private Coroutine blackBarEffectRoutine;
    private Tween topEffectTween;
    private Tween bottomEffectTween;
    private IEnumerator EnableBlackBarEffect(float duration)
    {
        Vector2 offset = GetBarOffset();

        topEffectTween = topBar.DOAnchorPos(topBarStartPosition.anchoredPosition - offset, duration / 2f).SetEase(Ease.OutQuad);
        bottomEffectTween = bottomBar.DOAnchorPos(bottomBarStartPosition.anchoredPosition + offset, duration / 2f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(duration);
        blackBarEffectRoutine = null;
    }

    private IEnumerator DisableBlackBarEffect(float duration)
    {
        topEffectTween = topBar.DOAnchorPos(topBarStartPosition.anchoredPosition, duration / 2f).SetEase(Ease.OutQuad);
        bottomEffectTween = bottomBar.DOAnchorPos(bottomBarStartPosition.anchoredPosition, duration / 2f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(duration);
        blackBarEffectRoutine = null;
    }

    private void ResetBlackBarEffect()
    {
        topEffectTween?.Kill();
        bottomEffectTween?.Kill();
        topBar.anchoredPosition = topBarStartPosition.anchoredPosition;
        bottomBar.anchoredPosition = bottomBarStartPosition.anchoredPosition;
    }
    public void ToggleInterface(bool toggle)
    {
        m_Container.SetActive(toggle);
        ToggleAreaNameDisplay(toggle);
        UIManager.Instance.comboUIController.ToggleInterface(toggle);
        UIManager.Instance.boostGaugeInterface.ToggleInterface(toggle);
    }

    public void ToggleAreaNameDisplay(bool toggle)
    {
        collectibleAreaName.enabled = toggle;
    }

    public void ToggleInterfaceAreaIntro(bool toggle)
    {
        m_Container.SetActive(toggle);
        UIManager.Instance.comboUIController.ToggleInterface(toggle);
        UIManager.Instance.boostGaugeInterface.ToggleInterface(toggle);
    }

    public void SunComboBehavior(int level)
    {
        if (sunImage == null) return;

        if (GameplayPaused())
            return;

        if (level == 0)
        {
            sunImage.transform.DOScale(sunScales[0], 0.3f).SetEase(Ease.InBack);
            return;
        }

        Debug.Log("Sun Combo Level: " + level);

        if (!sunImage.gameObject.activeSelf)
            sunImage.gameObject.SetActive(true);

        Vector3 targetScale = sunScales[Mathf.Clamp(level, 0, sunScales.Length - 1)];

        sunImage.transform
            .DOScale(targetScale, 0.35f)
            .SetEase(Ease.OutBack);

        sunImage.transform
            .DOPunchScale(Vector3.one * 0.2f, 0.3f, 4, 0.5f);
    }

    void SunOnBeatFever(int bar, int beat, float tempo)
    {
        if (sunImage == null) return;

        if (GameplayPaused())
            return;

        if (ComboManager.Instance.ComboLevel >= 4)
        {
            sunImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
        }

        if (speedGaugeObject == null) return;
        if (ComboManager.Instance.ComboLevel >= 4)
        {
            speedGaugeObject.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
        }
    }

        void SunOnBeat(int bar, int beat, float tempo)
    {
        if (sunImage == null) return;

        if (GameplayPaused())
            return;

        if (ComboManager.Instance.ComboLevel < 4)
        {
            sunImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
        }

        if (speedGaugeObject == null) return;
        if (ComboManager.Instance.ComboLevel < 4)
        {
            speedGaugeObject.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
        }
    }

    public void DebugToggleVisualCount(bool toggle)
    {
        coinUiVisual.SetActive(toggle);
        clamUiVisual.SetActive(toggle);
        buckieUiVisual.SetActive(toggle);
    }

    public void RefreshAreaClamCount()
    {
        if (CollectibleAreaManager.CurrentArea == null)
        {
            clamText.text = "";
            clamAreaCount.text = "";
            return;
        }

        clamText.text = "<sketchy>" + CollectibleAreaManager.CurrentArea.GetClamProgressText();
        clamAreaCount.text = "<sketchy>" + CollectibleAreaManager.CurrentArea.GetClamProgressText();
    }

    public void RefreshAreaBuckieCount()
    {
        if (CollectibleAreaManager.CurrentArea == null)
        {
            buckieAreaCount.text = "";
            buckieText.text = "";
            return;
        }
        buckieText.text = "<sketchy>" + CollectibleAreaManager.CurrentArea.GetBuckieProgressText();
        buckieAreaCount.text = "<sketchy>" + CollectibleAreaManager.CurrentArea.GetBuckieProgressText();
    }

    public void RefreshAreaSunCount()
    {
        Debug.Log("SUN JE SUIS LA");
        if (CollectibleAreaManager.CurrentArea == null)
        {
            SunAreaCount.text = "";
            sunText.text = "";
            return;
        }
        sunText.text = "<sketchy>" + CollectibleAreaManager.CurrentArea.GetSunProgressText();
        SunAreaCount.text = "<sketchy>" + CollectibleAreaManager.CurrentArea.GetSunProgressText();
    }

    public void RefreshAllAreaCount()
    {
        RefreshAreaSunCount();
        RefreshAreaBuckieCount();
        RefreshAreaClamCount();
    }

    public void UpdateAreaName(string name)
    {
        Debug.Log("NEW COLLIDER AREA IS" +  name);
        collectibleAreaName.text = name;
        collectibleAreaNameMenu.text = name;
    }

    public void ShowEndScreen()
    {
        //Show end screen
        ShowEndScreenRoutine = StartCoroutine(ShowEndScreenCoroutine());
    }

    private Coroutine ShowEndScreenRoutine;
    public IEnumerator ShowEndScreenCoroutine()
    {
        yield return null;
        ToggleInterface(false);
        Game.Instance.player.ForceLock(true);

        endScreenVisual.gameObject.SetActive(true);
        endScreenVisual.transform.localScale = Vector3.zero;
        endScreenVisual.transform.DOScale(endScreenOriginalScale, 1.2f).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(4f);
        ShowEndScreenRoutine = null;
        endScreenInteractText.SetActive(true);
    }

    public void DisableEndScreen()
    {
        if(endScreenVisual.gameObject.activeSelf == true)
        {
            //Disable end screen
            endScreenVisual.gameObject.SetActive(false);
            Game.Instance.player.ForceLock(false);
            ToggleInterface(true);
        }
    }

    private void Update()
    {
        if (interactAction.IsPressed())
        {
            if(ShowEndScreenRoutine == null)
            {
                DisableEndScreen();
            }
        }
    }
}
