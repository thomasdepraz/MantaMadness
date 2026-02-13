using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DialogLoader : MonoBehaviour
{
    public enum Languages { English, French}

    [Header("Language Settings")]
    [SerializeField] private Languages language = Languages.English;

    public static Languages CurrentLanguage { get; private set; }

    public static Dictionary<string, DialogueEntry> dialogues = new Dictionary<string, DialogueEntry>();

    public static event Action<Languages> OnLanguageChanged;

    public enum InputDeviceType { KeyboardMouse, Xbox, PlayStation }

    private const string PREF_LANG = "language";

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(_instance);
            _instance = this;
            return;
        }
        else
        {
            _instance = this;
        }

        var saved = PlayerPrefs.GetInt(PREF_LANG, (int)language);
        language = (Languages)Mathf.Clamp(saved, 0, Enum.GetValues(typeof(Languages)).Length - 1);

        LoadLanguage(language, notify: false);
    }

    private static DialogLoader _instance;

    public static void LoadLanguage(Languages newLanguage, bool notify = true)
    {
        CurrentLanguage = newLanguage;
        PlayerPrefs.SetInt(PREF_LANG, (int)newLanguage);

        dialogues.Clear();

        TextAsset tsvFile = Resources.Load<TextAsset>("Localization");
        if (tsvFile == null)
        {
            Debug.LogError("Localization file not found in Resources (Localization.txt)!");
            return;
        }

        string[] lines = tsvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            var cols = lines[i].Split('\t');

            for (int c = 0; c < cols.Length; c++)
                cols[c] = cols[c].Replace("\r", "").Trim();

            if (cols.Length < 5) continue;

            string dialogKey = cols[0];
            string dialogSpeaker = cols[1];

            string dialogText = newLanguage switch
            {
                Languages.English => cols[2],
                Languages.French => cols[3],
                _ => cols[2]
            };

            if (!dialogues.ContainsKey(dialogKey))
            {
                dialogues.Add(dialogKey, new DialogueEntry
                {
                    key = dialogKey,
                    speaker = dialogSpeaker,
                    dialog = dialogText
                });
            }
        }

        if (notify)
            OnLanguageChanged?.Invoke(newLanguage);

        Debug.Log("Langue = " + newLanguage);
    }

    public static DialogueEntry GetText(string key)
    {
        return dialogues.ContainsKey(key)
            ? dialogues[key]
            : new DialogueEntry { key = key, dialog = $"[Missing: {key}]", speaker = "Unknown" };
    }

    public static string ParseInputs(string text)
    {
        return Regex.Replace(text, @"\{(.*?)\}", match =>
        {
            string inputKey = match.Groups[1].Value;
            return InputLocalization.GetInput(inputKey);
        });
    }
}

public class DialogueEntry
{
    public string key;
    public string dialog;
    public string speaker;
}
