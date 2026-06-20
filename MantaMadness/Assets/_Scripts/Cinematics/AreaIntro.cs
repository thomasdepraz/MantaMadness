using DG.Tweening;
using FMOD.Studio;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class AreaIntro : MonoBehaviour, IDataPersistence
{
    [System.Serializable]
    public struct IntroStep
    {
        [Header("Camera")]
        public CinemachineCamera camera;

        [Header("Movement")]
        public Vector3 moveOffset;
        public bool useLocalMove;

        [Header("Timing")]
        public float duration;

        [Header("Tween")]
        public Ease ease;
        public CinemachineBlendDefinition blend;
    }

    [Header("Save")]
    [SerializeField] private string areaIntroId;

    [Header("Intro Steps Animations")]
    public List<IntroStep> introSteps = new List<IntroStep>();

    [SerializeField] private bool playOnlyOnce = true;

    private bool hasBeenActivated = false;
    public bool CanPlay()
    {
        return !playOnlyOnce || !hasBeenActivated;
    }

    public void Play()
    {
        if (!CanPlay()) return;

        hasBeenActivated = true;
        AreaIntroManager.Instance.PlayIntro(this);
    }


    public void LoadData(GameData data)
    {
        //TO FILL
        data.introAreaCinematic.TryGetValue(areaIntroId, out hasBeenActivated);
    }

    public void SaveData(ref GameData data)
    {
        //TO FILL
        //data.introarea dictionnary = hasbeenactivated
        if (data.introAreaCinematic.ContainsKey(areaIntroId))
        {
            data.introAreaCinematic.Remove(areaIntroId);
        }
        data.introAreaCinematic.Add(areaIntroId, hasBeenActivated);
    }
}
