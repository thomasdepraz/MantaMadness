using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Timeline;
using TMPro;
using FMODUnity;
[CreateAssetMenu]
public class DialogAsset : ScriptableObject
{
    [Header("Dialog Parameter")]
    public string key;

    public float delayBeforeTextBox;
    public float typingSpeed;
    public TimelineAsset cinematic;
    [TextArea]
    public string speakerName;
    [TextArea]
    public string dialogText;

    [Header("Font Mat")]
    public TMPro.TMP_FontAsset speakerMat;
    public TMPro.TMP_FontAsset dialogMat;

    [Header("Fmod sound info")]
    [SerializeField] public EventReference dialogSound;

}
