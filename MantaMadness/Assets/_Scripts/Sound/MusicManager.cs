using UnityEngine;
using FMODUnity;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public string EventName = "";
    public FMOD.Studio.EventInstance audioEvent;
    private float parameter = 0;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        audioEvent = RuntimeManager.CreateInstance(EventName);
        audioEvent.start();
    }

    private void FixedUpdate()
    {
        audioEvent.setParameterByName("Underwater", parameter);
    }

    public void ToggleUnderwater()
    {
        if(parameter == 0)
        {
            parameter = 1;
        }
        else
        {
            parameter = 0;
        }
    }

    private bool isPaused = false;

    public void ToggleMusic()
    {
        if (isPaused == false)
        {
            isPaused = true;
            audioEvent.setPaused(true);
        }
        else
        {
            isPaused =false;
            audioEvent.setPaused(false);
        }
    }
}
