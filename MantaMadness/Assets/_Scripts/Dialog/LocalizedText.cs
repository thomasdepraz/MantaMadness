using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    [SerializeField] private bool parseInputs = false;

    private TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        DialogLoader.OnLanguageChanged += OnLanguageChanged;

        Refresh();
    }

    void OnDisable()
    {
        DialogLoader.OnLanguageChanged -= OnLanguageChanged;
    }

    void Start()
    {
        Refresh();
    }

    private void OnLanguageChanged(DialogLoader.Languages lang)
    {
        Refresh();
    }

    public void Refresh()
    {
        var entry = DialogLoader.GetText(localizationKey);
        string value = entry.dialog;

        if (parseInputs)
            value = DialogLoader.ParseInputs(value);

        var inputLocalized = GetComponent<TMP_InputLocalizedText>();
        if (inputLocalized != null)
            inputLocalized.SetSourceText(value);
        else
            text.text = value;
    }
}


