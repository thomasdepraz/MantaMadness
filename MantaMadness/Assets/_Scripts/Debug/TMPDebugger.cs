using UnityEngine;
using TMPro;
using System.Diagnostics;

public class TMPDebugger : MonoBehaviour
{
    private TMP_Text tmp;
    private string lastText;

    void Awake()
    {
        tmp = GetComponent<TMP_Text>();
        lastText = tmp.text;

        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
    }

    void OnDestroy()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
    }

    void OnTextChanged(Object obj)
    {
        if (obj != tmp) return;
        if (tmp.text == lastText) return;

        lastText = tmp.text;
    }
}