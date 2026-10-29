using UnityEngine;

[CreateAssetMenu]
public class TutorialAsset : ScriptableObject
{
    [Header("Dialog Parameter")]
    public string key;

    [TextArea]
    public string dialogText;
    public TMPro.TMP_FontAsset dialogMat;
    public TutorialType type;
}
