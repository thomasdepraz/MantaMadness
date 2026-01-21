using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class DialogLoader : MonoBehaviour
{
    public enum Languages
    {
        English,
        French,
        Italian
    }
    [Header("Language Settings")]
    [SerializeField]private Languages language;
    public static Dictionary<string, DialogueEntry> dialogues = new Dictionary<string, DialogueEntry>();

    public enum InputDeviceType
    {
        KeyboardMouse,
        Xbox,
        PlayStation,
    }

    void Awake()
    {
        dialogues.Clear();
        TextAsset csvFile = Resources.Load<TextAsset>("Localization");
        string[] lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 5) continue;

            string dialogKey = cols[0].Trim();
            string dialogSpeaker = cols[1].Trim();

            string dialogText = "";
            switch (language)
            {
                case Languages.English:
                    dialogText = cols[2];
                    break;
                case Languages.French:
                    dialogText = cols[3];
                    break;
                case Languages.Italian:
                    dialogText = cols[4];
                    break;
                default:
                    //Default to English
                    dialogText = cols[2];
                    break;
            }

            if (!dialogues.ContainsKey(dialogKey))
            {
                dialogues.Add(dialogKey, new DialogueEntry { key = dialogKey, speaker = dialogSpeaker, dialog = dialogText});
            }
        }

        Debug.Log("Langue = " + language);
    }

    public static DialogueEntry GetText(string key)
    {
        return dialogues.ContainsKey(key) ? dialogues[key]: new DialogueEntry { key = key, dialog = $"[Missing: {key}]", speaker = "Unknown" };
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
