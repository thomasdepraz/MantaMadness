using FMODUnity;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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

public enum MUSICS
{
    MUSIC_MENU,
    MUSIC_CAVE,
    MUSIC_LEVEL01,
    MUSIC_LEVEL02,
    MUSIC_LEVEL03,
    NULL,
}

public class MusicManager : MonoBehaviour, IDataPersistence
{
    public static MusicManager Instance;

    public EventReference music_menu, music_cave, music_level01, music_level02, music_level03, music_null;
    public FMOD.Studio.EventInstance audioEvent;

    private float parameter = 0;

    private GCHandle thisHandle;
    private FMOD.Studio.EVENT_CALLBACK beatCallback;

    public static event Action<int, int, float> OnBeat;
    private MUSICS currentMusic;

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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void LoadData(GameData data)
    {
        Debug.Log("LOAD MUSIC = " + data.gameStartMusic);
        currentMusic = data.gameStartMusic;
    }

    public void SaveData(ref GameData data)
    {
        Debug.Log("SAVE MUSIC = " + currentMusic);
        data.gameStartMusic = currentMusic;
    }

    private void Start()
    {   
        ApplyMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyMusicForScene(scene.name);
    }

    private void ApplyMusicForScene(string sceneName)
    {
        //Stop music
        if (audioEvent.isValid())
        {
            audioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            audioEvent.release();
        }

        if (sceneName == "MainMenu")
        {
            audioEvent = RuntimeManager.CreateInstance(music_menu);
        }
        else if (sceneName == "Main")
        {
            switch (currentMusic)
            {
                case MUSICS.MUSIC_CAVE:
                    audioEvent = RuntimeManager.CreateInstance(music_cave);
                    break;
                case MUSICS.MUSIC_LEVEL01:
                    audioEvent = RuntimeManager.CreateInstance(music_level01);
                    break;
                case MUSICS.MUSIC_LEVEL02:
                    audioEvent = RuntimeManager.CreateInstance(music_level02);
                    break;
                case MUSICS.MUSIC_LEVEL03:
                    audioEvent = RuntimeManager.CreateInstance(music_level03);
                    break;
                case MUSICS.NULL:
                    break;
            }
        }

        // Rebind callback FMOD
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
        audioEvent.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        if(thisHandle.IsAllocated)
            thisHandle.Free();

        thisHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
        audioEvent.setUserData(GCHandle.ToIntPtr(thisHandle));

        audioEvent.start();
    }

    private void FixedUpdate()
    {
        if(audioEvent.isValid())
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

    public void PlayMusic(MUSICS newMusic)
    {
        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        if(currentMusic != newMusic)
        musicCoroutine = StartCoroutine(PlayMusicWithDelayCoroutine(newMusic, 1.5f));
    }

    private Coroutine musicCoroutine;

    private IEnumerator PlayMusicWithDelayCoroutine(MUSICS newMusic, float delay)
    {
        // 1. Fade out la musique actuelle
        if (audioEvent.isValid())
        {
            audioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            audioEvent.release();
        }

        // 2. Attente
        yield return new WaitForSeconds(delay);

        // 3. Mise à jour de la musique courante
        currentMusic = newMusic;

        // 4. Création du nouvel EventInstance
        switch (currentMusic)
        {
            case MUSICS.MUSIC_MENU:
                audioEvent = RuntimeManager.CreateInstance(music_menu);
                break;

            case MUSICS.MUSIC_CAVE:
                audioEvent = RuntimeManager.CreateInstance(music_cave);
                break;

            case MUSICS.MUSIC_LEVEL01:
                audioEvent = RuntimeManager.CreateInstance(music_level01);
                break;

            case MUSICS.MUSIC_LEVEL02:
                audioEvent = RuntimeManager.CreateInstance(music_level02);
                break;

            case MUSICS.MUSIC_LEVEL03:
                audioEvent = RuntimeManager.CreateInstance(music_level03);
                break;
        }

        // 5. Rebind du callback FMOD (IMPORTANT)
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
        audioEvent.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        if (thisHandle.IsAllocated)
            thisHandle.Free();

        thisHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
        audioEvent.setUserData(GCHandle.ToIntPtr(thisHandle));

        // 6. Play
        audioEvent.start();
    }

}

