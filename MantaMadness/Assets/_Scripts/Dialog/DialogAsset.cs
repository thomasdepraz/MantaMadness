using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Timeline;
using TMPro;
[CreateAssetMenu]
public class DialogAsset : ScriptableObject
{
    public string key;

    public float delayBeforeTextBox;
    public float typingSpeed;
    public TimelineAsset cinematic;
    [TextArea]
    public string speakerName;
    [TextArea]
    public string dialogText;

    public TMPro.TMP_FontAsset speakerMat;
    public TMPro.TMP_FontAsset dialogMat;

}
