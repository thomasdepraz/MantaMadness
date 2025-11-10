using Unity.Cinemachine;
using UnityEngine;
[CreateAssetMenu]
public class DialogAsset : ScriptableObject
{
    public float delayBeforeTextBox;
    public float typingSpeed;
    public CinemachineCamera virtualCam;
    public Transform camTransform;
    [TextArea]
    public string speakerName;
    [TextArea]
    public string dialogText;
}
