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

    public void PlayCinematic(TimelineAsset cinematic)
    {
        ResetCam();
        cinematicPlayer.Play(cinematic);
        MantaCameraController.instance.DeactivatePlayerCamera();
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
}
