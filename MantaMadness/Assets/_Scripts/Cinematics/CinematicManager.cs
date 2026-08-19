using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

public class CinematicManager : MonoBehaviour
{
    public static CinematicManager instance;

    public PlayableDirector cinematicPlayer;
    public List<CinemachineCamera>cinematicCameras = new List<CinemachineCamera>();

    public bool isCinematicPlaying;

    public PlayableDirector gameIntroCinematicPlayer;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        //cinematicPlayer.stopped += OnCinematicFinished;
    }

    public void PlayCinematic(TimelineAsset cinematic)
    {
        ResetCam();
        cinematicPlayer.Play(cinematic);
        MantaCameraController.instance.DeactivatePlayerCamera();

        isCinematicPlaying = true;
    }


    public void EndCinematic()
    {
        if(cinematicPlayer.time != cinematicPlayer.duration)
        {
            cinematicPlayer.time = cinematicPlayer.duration;
            cinematicPlayer.Evaluate();
        }
        cinematicPlayer.Stop();
        cinematicPlayer.playableAsset = null;
        ResetCam();
        MantaCameraController.instance.ActivatePlayerCamera();
    }

    public void ResetCam()
    {
        foreach(CinemachineCamera cam in cinematicCameras)
        {
            cam.gameObject.SetActive(false);
        }
    }

    private void OnCinematicFinished(PlayableDirector director)
    {
        EndCinematic();
    }

    public void PlayIntroCinematic()
    {
        isCinematicPlaying = true;

        ResetCam();
        UIManager.Instance.gameInterface.ToggleInterfaceAreaIntro(false);
        gameIntroCinematicPlayer.Play();
        MantaCameraController.instance.DeactivatePlayerCamera();
    }

    public void Update()
    {
        if(cinematicPlayer.state == PlayState.Playing || gameIntroCinematicPlayer.state == PlayState.Playing)
        {
            isCinematicPlaying = true;
        }
        else
        {
            isCinematicPlaying = false;
        }
    }
}
