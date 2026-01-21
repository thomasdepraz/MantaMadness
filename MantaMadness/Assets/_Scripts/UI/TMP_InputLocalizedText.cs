using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using static DialogLoader;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMP_InputLocalizedText : MonoBehaviour
{
    private TextMeshProUGUI tmp;

    [TextArea]
    [SerializeField]
    private string rawText; // "Press {INTERACT}"

    private static readonly Regex inputRegex = new(@"\{(.*?)\}");

    private void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();

        if (string.IsNullOrEmpty(rawText))
            rawText = tmp.text; // capturé UNE FOIS
    }

    private void OnEnable()
    {
        Refresh();
        InputManager.OnDeviceChanged += OnDeviceChanged;
    }

    private void OnDisable()
    {
        InputManager.OnDeviceChanged -= OnDeviceChanged;
    }

    private void OnDeviceChanged(InputDeviceType device)
    {
        Refresh();
    }

    public void Refresh()
    {
        if (string.IsNullOrEmpty(rawText)) return;

        tmp.text = inputRegex.Replace(rawText, match =>
        {
            string key = match.Groups[1].Value.Trim();
            return InputLocalization.GetInput(key);
        });
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (tmp == null)
            tmp = GetComponent<TextMeshProUGUI>();

        Refresh();
    }
#endif
}

