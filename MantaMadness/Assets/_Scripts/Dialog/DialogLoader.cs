using UnityEngine;
using System.Collections.Generic;
using System.IO;

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
    public static Dictionary<string, string> dialogues = new Dictionary<string, string>();

    void Awake()
    {
        TextAsset csvFile = Resources.Load<TextAsset>("Localization");
        string[] lines = csvFile.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length < 2) continue;
            switch (language)
            {
                case Languages.English:
                    dialogues[cols[0]] = cols[2];
                    break;
                case Languages.French:
                    dialogues[cols[0]] = cols[3];
                    break;
                case Languages.Italian:
                    dialogues[cols[0]] = cols[4];
                    break;
                default:
                    //Default to English
                    dialogues[cols[0]] = cols[2];
                    break;
            }
        }

        Debug.Log("Langue = " + language);
    }

    public static string GetText(string key)
    {
        return dialogues.ContainsKey(key) ? dialogues[key] : $"[Missing: {key}]";
    }
}
