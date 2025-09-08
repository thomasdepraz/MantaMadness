using UnityEngine;
using FMODUnity;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    public EventReference eventRef;
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
        audioEvent = RuntimeManager.CreateInstance(eventRef);
        audioEvent.start();
    }

    private void FixedUpdate()
    {
        audioEvent.setParameterByName("Underwater", parameter);
    }

    public void ToggleUnderwater()
    {
        FmodGlobalParameters.instance.ToggleGlobalParameter(FmodGlobalParamName.G_Player_Underwater);
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
