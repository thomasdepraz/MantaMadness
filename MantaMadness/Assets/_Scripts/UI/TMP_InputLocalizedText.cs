using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static DialogLoader;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMP_InputLocalizedText : MonoBehaviour
{
    private TextMeshProUGUI tmp;

    [TextArea]
    [SerializeField] private string sourceText;

    private static readonly Regex inputRegex = new(@"\{(.*?)\}");

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        InputManager.OnDeviceChanged += OnDeviceChanged;
        Refresh();
    }

    private void OnDisable()
    {
        InputManager.OnDeviceChanged -= OnDeviceChanged;
    }

    private void OnDeviceChanged(InputDeviceType device)
    {
        Refresh();
    }

    public void SetSourceText(string localizedText)
    {
        sourceText = localizedText;
        Refresh();
    }

    public void Refresh()
    {
        if (string.IsNullOrEmpty(sourceText))
            return;

        tmp.text = inputRegex.Replace(sourceText, match =>
        {
            string key = match.Groups[1].Value.Trim();
            return InputLocalization.GetInput(key);
        });
    }
}

