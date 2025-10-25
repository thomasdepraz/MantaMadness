using FMODUnity;
using System;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

[StructLayout(LayoutKind.Sequential)]
public struct FMOD_STUDIO_TIMELINE_BEAT_PROPERTIES
{
    public int bar;
    public int beat;
    public int position;
    public float tempo;
    public int timeSignatureUpper;
    public int timeSignatureLower;
}

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public EventReference eventRef;
    public FMOD.Studio.EventInstance audioEvent;

    private float parameter = 0;

    private GCHandle thisHandle;
    private FMOD.Studio.EVENT_CALLBACK beatCallback;

    public static event Action<int, int, float> OnBeat;

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void EditorStopAllFMOD()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("Stopping all FMOD events before exiting Play Mode...");
                FMODUnity.RuntimeManager.StudioSystem.flushCommands();
                FMODUnity.RuntimeManager.StudioSystem.release();
            }
        };
    }
#endif


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        audioEvent = RuntimeManager.CreateInstance(eventRef);

        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
        audioEvent.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        thisHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
        audioEvent.setUserData(GCHandle.ToIntPtr(thisHandle));

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
            isPaused = false;
            audioEvent.setPaused(false);
        }
    }

    private static FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        if (type == FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
        {
            // récupérer le user data
            FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);
            IntPtr userData;
            instance.getUserData(out userData);

            if (userData != IntPtr.Zero)
            {
                GCHandle handle = GCHandle.FromIntPtr(userData);
                var manager = handle.Target as MusicManager;

                // lire la struct
                FMOD_STUDIO_TIMELINE_BEAT_PROPERTIES beat =
                    (FMOD_STUDIO_TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(
                        parameterPtr, typeof(FMOD_STUDIO_TIMELINE_BEAT_PROPERTIES));

                // Notifie les listeners Unity
                OnBeat?.Invoke(beat.bar, beat.beat, beat.tempo);
            }
        }

        return FMOD.RESULT.OK;
    }

    private void OnDestroy()
    {
        if (audioEvent.isValid())
        {
            audioEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            audioEvent.release();
        }

        if (thisHandle.IsAllocated)
            thisHandle.Free();
    }
}

