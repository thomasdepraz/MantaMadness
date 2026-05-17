using TMPEffects.Components;
using TMPro;
using UnityEngine;
using System.Collections;
using DG.Tweening;

public class ComboUIController : MonoBehaviour, IScreen
{
    //[SerializeField] TMP_Text comboValueText;

    [SerializeField] GameObject Root;
    [SerializeField] GameObject feverBorder;
    [SerializeField] GameObject feverRoot;
    [SerializeField] GameObject timerVisualContainer;
    [SerializeField] Transform timerScaleObject;
    [SerializeField] private ParticleSystem timerParticle;

    [SerializeField] private ParticleSystem feverOverlayParticle;
    [SerializeField] private GameObject[] feverMainVisualEffect;

    [SerializeField] private TMPAnimator tmpAnimator;
    [SerializeField] TMP_Text comboNameText;
    [SerializeField] private float stepDuration = 0.5f;
    [SerializeField, Range(0f, 0.9f)] private float overlapPercent = 0.4f;

    private Coroutine feverRoutine;
    private Vector3 timerObjectInitialScale;
    private Vector3 ComboTextInitialScale;

    private TMP_FontAsset defaultFont;
    private Material defaultMaterial;

    public GameObject Container => gameObject;

    static string EffectToTag(ComboEffectPreset effect)
    {
        return effect switch
        {
            ComboEffectPreset.wave => "wave",
            ComboEffectPreset.shake => "shake",
            ComboEffectPreset.palette => "palette",
            ComboEffectPreset.grow => "grow",
            ComboEffectPreset.fade => "fade",
            ComboEffectPreset.funky => "funky",
            ComboEffectPreset.dangle=> "dangle",
            ComboEffectPreset.shear => "shear",
            ComboEffectPreset.jump => "jump",
            ComboEffectPreset.pivot => "pivot",
            ComboEffectPreset.pivotc=> "pivotc",
            ComboEffectPreset.sketchy => "sketchy",
            ComboEffectPreset.spread => "spread",
            ComboEffectPreset.swing => "swing",
            _ => null
        };
    }

    public void ToggleInterface(bool toggle)
    {
        if (!toggle)
        {
            StopFeverSequence();
        }

        gameObject.SetActive(toggle);
    }

    void Start()
    {
        if (UIManager.Instance.comboUIController == null)
        {
            UIManager.Instance.comboUIController = this;
        }


        //Setup
        if (ComboManager.Instance.State == ComboState.Inactive)
        {
            Hide();
        }
        else
        {
            Show();
        }

        Sync();

        timerObjectInitialScale = timerScaleObject.localScale;
        ComboTextInitialScale = comboNameText.gameObject.transform.localScale;
        defaultFont = comboNameText.font;
        defaultMaterial = comboNameText.fontMaterial;
    }

    private void OnEnable()
    {
        StartCoroutine(DelaySetup());
    }

    private IEnumerator DelaySetup()
    {
        yield return new WaitForSeconds(0.1f);
        ComboManager.Instance.OnComboStarted += Show;
        ComboManager.Instance.OnComboEnded += Hide;
        ComboManager.Instance.OnComboValueChanged += UpdateValue;
        ComboManager.Instance.OnActionAdded += UpdateActionName;
        ComboManager.Instance.OnStateChanged += OnStateChanged;

        MusicManager.OnBeat += OnBeatEnableFever;
        MusicManager.OnBeat8 += OnBeatPlayParticle;

        //Setup
        if (ComboManager.Instance.State == ComboState.Inactive)
        {
            Hide();
        }
        else
        {
            Show();
        }

        Sync();
        StopFeverSequence();
    }

    private void OnDisable()
    {
        ComboManager.Instance.OnComboStarted -= Show;
        ComboManager.Instance.OnComboEnded -= Hide;
        ComboManager.Instance.OnComboValueChanged -= UpdateValue;
        ComboManager.Instance.OnActionAdded -= UpdateActionName;
        ComboManager.Instance.OnStateChanged -= OnStateChanged;

        MusicManager.OnBeat -= OnBeatEnableFever;
        MusicManager.OnBeat8 -= OnBeatPlayParticle;

        Root.SetActive(false);
        feverRoot.SetActive(false);
        feverBorder.SetActive(false);
        timerVisualContainer.SetActive(false);

        StopFeverSequence();
    }

    private void Update()
    {
        UpdateTimerScale();
    }

    void OnDestroy()
    {
        if (ComboManager.Instance == null)
            return;

        ComboManager.Instance.OnComboStarted -= Show;
        ComboManager.Instance.OnComboEnded -= Hide;
        ComboManager.Instance.OnComboValueChanged -= UpdateValue;
        ComboManager.Instance.OnActionAdded -= UpdateActionName;
        ComboManager.Instance.OnStateChanged -= OnStateChanged;

        MusicManager.OnBeat -= OnBeatEnableFever;
        MusicManager.OnBeat8 -= OnBeatPlayParticle;
    }

    void Show()
    {
        Root.SetActive(true);
        timerVisualContainer.SetActive(true);
    }

    void Hide()
    {
        Root.SetActive(false);
        timerVisualContainer.SetActive(false);
    }

    void UpdateValue(int value)
    {
        //comboValueText.text = value.ToString();
    }

    void UpdateActionName(ComboActionSO action)
    {
        comboNameText.gameObject.transform.DOKill();
        comboNameText.gameObject.transform.localScale = ComboTextInitialScale;
        comboNameText.gameObject.transform.DOScale(new Vector3(1.5f,1.5f,1.5f),0.15f).SetEase(Ease.OutQuad).SetLoops(2,LoopType.Yoyo);

        ApplyFont(action);

        comboNameText.text = BuildTMPEffectsText(action);

        // Important avec TMP : force update du mesh
        comboNameText.ForceMeshUpdate();
    }



    void OnStateChanged(ComboState state)
    {
        switch (state)
        {
            case ComboState.Inactive:
                Root.SetActive(false);
                feverRoot.SetActive(false);
                feverBorder.SetActive(false);
                break;

            case ComboState.Active:
                Root.SetActive(true);
                StopFeverSequence();
                feverRoot.SetActive(false);
                feverBorder.SetActive(false);
                break;

            case ComboState.Fever:
                Root.SetActive(true);
                break;
        }
    }

    void OnBeatEnableFever(int bar, int beat, float tempo)
    {
        if (Time.timeScale == 0f)
            return;

        if (ComboManager.Instance.State == ComboState.Fever && feverRoot.activeSelf != true)
        {
            feverRoot.SetActive(true);
            feverBorder.SetActive(true);
            FeverParticlesAndEffects();
        }
    }

    void OnBeatPlayParticle(int bar, int beat, float tempo)
    {
        if (Time.timeScale == 0f)
            return;

        if (ComboManager.Instance.State == ComboState.Fever && feverRoot.activeSelf == true)
        {
            FeverParticlesAndEffects();
        }
    }

    void FeverParticlesAndEffects()
    {
        feverOverlayParticle.Play();

        if (feverRoutine == null)
        {
            feverRoutine = StartCoroutine(FeverSequentialOverlap());
        }
    }

    private IEnumerator FeverSequentialOverlap()
    {
        if (feverMainVisualEffect == null || feverMainVisualEffect.Length == 0)
            yield break;

        float nextDelay = stepDuration * (1f - overlapPercent);

        int length = feverMainVisualEffect.Length;

        for (int i = 0; i < length; i++)
        {
            int currentIndex = i;
            int previousIndex = i - 1;

            // Active objet courant
            feverMainVisualEffect[currentIndex].SetActive(true);

            // Désactive l'objet précédent après le temps total
            if (previousIndex >= 0)
            {
                StartCoroutine(DisableAfterDelay(feverMainVisualEffect[previousIndex],stepDuration));
            }

            yield return new WaitForSeconds(nextDelay);
        }

        yield return new WaitForSeconds(stepDuration);

        ResetFeverVisuals();
    }

    private IEnumerator DisableAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);
    }

    private void ResetFeverVisuals()
    {
        if (feverMainVisualEffect == null)
            return;

        foreach(var obj in feverMainVisualEffect)
        {
            if (obj != null)
                obj.SetActive(false);
        }
        feverRoutine = null;
    }

    void StopFeverSequence()
    {
        if (feverRoutine != null)
        {
            StopCoroutine(feverRoutine);
            feverRoutine = null;
        }

        ResetFeverVisuals();
    }

    void Sync()
    {
        OnStateChanged(ComboManager.Instance.State);

        UpdateValue(ComboManager.Instance.CurrentValue);
    }

    void UpdateTimerScale()
    {
        if (ComboManager.Instance == null)
            return;

        ComboState state = ComboManager.Instance.State;

        // Si combo inactif scale à 0
        if (state == ComboState.Inactive)
        {
            SetScaleY(0f);
            StopTimerParticle();
            return;
        }

        if (ComboManager.Instance.IsBonusPhase)
        {
            SetScaleY(0f);
            StopTimerParticle();
            return;
        }

        float normalized = ComboManager.Instance.TimerNormalized;

        SetScaleY(normalized);
        PlayTimerParticle();
    }
    void PlayTimerParticle()
    {
        if (timerParticle != null && !timerParticle.isPlaying)
        {
            timerParticle.Play();
        }
    }

    void StopTimerParticle()
    {
        if (timerParticle != null && timerParticle.isPlaying)
        {
            timerParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    void SetScaleY(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);

        Vector3 scale = timerObjectInitialScale;

        scale.y *= normalized;

        timerScaleObject.localScale = scale;
    }

    string BuildTMPEffectsText(ComboActionSO action)
    {
        string tag = EffectToTag(action.effectPreset);

        if (string.IsNullOrEmpty(tag))
            return action.actionName;

        string parameters = BuildParameters(action);

        return $"<{tag}{parameters}>{action.actionName}</{tag}>";
    }

    string BuildParameters(ComboActionSO action)
    {
        switch (action.effectPreset)
        {
            case ComboEffectPreset.wave:
                return $" amplitude={action.amplitude} frequency={action.frequency} speed={action.speed}";

            case ComboEffectPreset.shake:
                return $" amplitude={action.amplitude} speed={action.speed}";

            case ComboEffectPreset.grow:
                return $" amplitude={action.amplitude} speed={action.speed}";

            case ComboEffectPreset.palette:
                return $" speed={action.speed}";

            case ComboEffectPreset.sketchy:
                return $" amplitude={action.amplitude} speed={action.speed}";

            case ComboEffectPreset.dangle:
                return $" amplitude={action.amplitude} speed={action.speed}";

            case ComboEffectPreset.shear:
                return $" amplitude={action.amplitude} speed={action.speed}";

            case ComboEffectPreset.swing:
                return $" amplitude={action.amplitude} frequency={action.frequency} speed={action.speed}";

            case ComboEffectPreset.spread:
                return $" amplitude={action.amplitude} speed={action.speed}";

            case ComboEffectPreset.funky:
                return $" amplitude={action.amplitude} frequency={action.frequency} speed={action.speed}";

            case ComboEffectPreset.jump:
                return $" amplitude={action.amplitude} speed={action.speed}";

            case ComboEffectPreset.fade:
                return $" speed={action.speed}";

            case ComboEffectPreset.pivot:
                return $" amplitude={action.amplitude} speed={action.speed}";

            case ComboEffectPreset.pivotc:
                return $" amplitude={action.amplitude} speed={action.speed}";

            case ComboEffectPreset.None:
            default:
                return "";
        }
    }

    void ApplyFont(ComboActionSO action)
    {
        if (action.fontOverride != null)
        {
            comboNameText.font = action.fontOverride;
            comboNameText.fontMaterial = action.fontOverride.material;
        }
        else
        {
            comboNameText.font = defaultFont;
            comboNameText.fontMaterial = defaultMaterial;
        }
    }

}
