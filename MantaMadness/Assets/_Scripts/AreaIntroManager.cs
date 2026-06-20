using DG.Tweening;
using FMOD.Studio;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class AreaIntroManager : MonoBehaviour
{
    //MANAGER of the area intros cinematic. it both plays and goes back to the player when done. So most of the logic is handled here

    public static AreaIntroManager Instance;

    [Header("Cameras")]
    [SerializeField] private CinemachineBlendDefinition blend;

    private bool isPlaying;
    private Coroutine currentIntroCoroutine;
    private CinemachineCamera previousCamera = null;

    private AreaIntro currentAreaIntro;

    public void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void PlayIntro(AreaIntro areaIntro)
    {
        if (isPlaying) return;

        currentAreaIntro = areaIntro;
        currentIntroCoroutine = StartCoroutine(PlayIntroRoutine(areaIntro));
    }

    private IEnumerator PlayIntroRoutine(AreaIntro areaIntro)
    {
        isPlaying = true;
        FogState savedState = WeatherManager.instance.currentFogState;
        WeatherManager.instance.UpdateFog(FogState.disabled);

        foreach (AreaIntro.IntroStep step in areaIntro.introSteps)
        {
            step.camera.gameObject.SetActive(true);

            CameraManager.Instance.BlendToCamera(step.camera, step.blend);

            yield return null;

            Sequence stepSequence = DOTween.Sequence();

            if (step.useLocalMove)
            {
                stepSequence.Join(
                    step.camera.transform
                        .DOLocalMove(
                            step.camera.transform.localPosition + step.moveOffset,
                            step.duration)
                        .SetEase(step.ease));
            }
            else
            {
                stepSequence.Join(
                    step.camera.transform
                        .DOMove(
                            step.camera.transform.position + step.moveOffset,
                            step.duration)
                        .SetEase(step.ease));
            }
            yield return stepSequence.WaitForCompletion();

            if (previousCamera != null)
            {
                previousCamera.gameObject.SetActive(false);
            }

            previousCamera = step.camera;
        }

        EndIntro(savedState);
    }

    private void EndIntro(FogState fogState)
    {

        WeatherManager.instance.UpdateFog(fogState);

        if (currentAreaIntro != null)
        {
            foreach (var step in currentAreaIntro.introSteps)
            {
                step.camera.transform.localPosition = Vector3.zero;
                step.camera.transform.localRotation = Quaternion.identity;

                CameraManager.Instance.ResetCamera(step.camera);
            }
        }

        isPlaying = false;
        currentIntroCoroutine = null;
        currentAreaIntro = null;
    }

    //public void SkipIntro()
    //{
    //    if (!isPlaying) return;

    //    currentSequence?.Kill();
    //    EndIntro();
    //}

}
