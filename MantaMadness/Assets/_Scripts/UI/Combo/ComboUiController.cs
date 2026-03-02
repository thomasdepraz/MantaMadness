using TMPEffects.Components;
using TMPro;
using UnityEngine;

public class ComboUIController : MonoBehaviour
{
    [SerializeField] TMP_Text comboValueText;

    [SerializeField] GameObject Root;
    [SerializeField] GameObject comboContainer;
    [SerializeField] GameObject feverContainer;
    [SerializeField] GameObject timerVisualContainer;
    [SerializeField] Transform timerScaleObject;

    [SerializeField] private TMPAnimator tmpAnimator;
    [SerializeField] TMP_Text comboNameText;

    private Vector3 timerObjectInitialScale;

    private TMP_FontAsset defaultFont;
    private Material defaultMaterial;

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

    void Start()
    {
        ComboManager.Instance.OnComboStarted += Show;
        ComboManager.Instance.OnComboEnded += Hide;
        ComboManager.Instance.OnComboValueChanged += UpdateValue;
        ComboManager.Instance.OnActionAdded += UpdateActionName;
        ComboManager.Instance.OnStateChanged += OnStateChanged;


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
        defaultFont = comboNameText.font;
        defaultMaterial = comboNameText.fontMaterial;
    }

    private void OnEnable()
    {
        ComboManager.Instance.OnComboStarted += Show;
        ComboManager.Instance.OnComboEnded += Hide;
        ComboManager.Instance.OnComboValueChanged += UpdateValue;
        ComboManager.Instance.OnActionAdded += UpdateActionName;
        ComboManager.Instance.OnStateChanged += OnStateChanged;


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
        comboValueText.text = value.ToString();
    }

    void UpdateActionName(ComboActionSO action)
    {

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
                break;

            case ComboState.Active:
                Root.SetActive(true);
                comboContainer.SetActive(true);
                feverContainer.SetActive(false);
                break;

            case ComboState.Fever:
                Root.SetActive(true);
                comboContainer.SetActive(false);
                feverContainer.SetActive(true);
                break;
        }
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
            return;
        }

        float normalized = ComboManager.Instance.TimerNormalized;

        SetScaleY(normalized);
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
