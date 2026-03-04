using DG.Tweening;
using EasyTextEffects;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class GameInterface : MonoBehaviour, IScreen
{
    public GameObject Container => m_Container;
    public GameObject m_Container;

    public Camera uiCamera;
    [Header("UI Ressources Parameters")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI clamText;

    [Header("Sun Overlay Parameters")]
    public Image sunImage;
    public Image sunOverlay;
    [SerializeField] private Vector3[] sunScales =
    {
        Vector3.one * 0.5f,
        Vector3.one * 0.6f,
        Vector3.one * 0.7f,
        Vector3.one * 0.8f,
        Vector3.one, //FEVER
    };

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


    public void Start()
    {
        UIManager.Instance.gameInterface = this;
        CameraManager.Instance.AddCameraToStack(uiCamera);

        CoinManager.Instance.coinPickedUp += UpdateCoinCount;
        CoinManager.Instance.collectiblePickedUp += UpdateCollectibleCount;
        coinText.text = CoinManager.Instance.PickupCoinCount.ToString();
        ComboManager.Instance.OnComboLevelChanged += SunComboBehavior;
        MusicManager.OnBeat += SunOnBeatFever;
        MusicManager.OnBeat2 += SunOnBeat;

        textRect = text.GetComponent<RectTransform>();
        textEffects.StartManualEffects();
        text.enabled = false;

        if(sunOverlay.IsActive() == true)
        {
            sunOverlay.enabled = false;
        }

        if(catOverlay.IsActive() == true)
        {
            catOverlay.enabled = false;
        }

        sunImage.transform.localScale = sunScales[0];
    }

    private void OnDestroy()
    {
        CoinManager.Instance.coinPickedUp -= UpdateCoinCount;
        CoinManager.Instance.collectiblePickedUp -= UpdateCollectibleCount;
        ComboManager.Instance.OnComboLevelChanged -= SunComboBehavior;
        MusicManager.OnBeat -= SunOnBeatFever;
        MusicManager.OnBeat2 -= SunOnBeat;
        Debug.Log("GameInterface destroyed");
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
    }

    public void SunComboBehavior(int level)
    {
        if (sunImage == null) return;

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

        if (ComboManager.Instance.ComboLevel >= 4)
        {
            sunImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
        }
    }

    void SunOnBeat(int bar, int beat, float tempo)
    {
        if (sunImage == null) return;

        if (ComboManager.Instance.ComboLevel < 4)
        {
            sunImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f);
        }
    }
}
