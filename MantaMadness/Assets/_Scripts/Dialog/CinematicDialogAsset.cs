using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Timeline;
using TMPro;
using FMODUnity;
[CreateAssetMenu]

public class CinematicDialogAsset : ScriptableObject
{
    [Header("Dialog Parameter")]
    public string key;

    public float timeToShowText;
    public float timeToLinger;

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
