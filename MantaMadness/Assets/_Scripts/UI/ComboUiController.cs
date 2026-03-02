using TMPro;
using UnityEngine;

public class ComboUIController : MonoBehaviour
{
    [SerializeField] TMP_Text comboValueText;
    [SerializeField] TMP_Text comboNameText;
    [SerializeField] GameObject Root;
    [SerializeField] GameObject comboContainer;
    [SerializeField] GameObject feverContainer;
    [SerializeField] GameObject timerVisualContainer;
    [SerializeField] Transform timerScaleObject;

    private Vector3 timerObjectInitialScale;

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

    void UpdateActionName(ComboAction action)
    {
        comboNameText.text = action.Name;
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
}
