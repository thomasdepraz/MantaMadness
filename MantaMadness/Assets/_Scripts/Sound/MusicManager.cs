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
    MUSIC_UFO,
    MUSIC_BACKROOM,
    NULL,
}

public enum AMBIENT
{
    NULL,
    AMB_BEACH,
    AMB_SECRET,
    AMB_CITY,
    AMB_VOLCANO,
}

public class MusicManager : MonoBehaviour, IDataPersistence
{
    public static MusicManager Instance;


    public EventReference music_menu, music_cave, music_level01, music_level02, music_level03, music_null, music_UFO, music_backroom;
    public FMOD.Studio.EventInstance musicAudioEvent;

    public EventReference amb_beach, amb_secret, amb_city, amb_volcano;
    public FMOD.Studio.EventInstance ambientAudioEvent;

    private float parameter = 0;

    private GCHandle thisHandle;
    private FMOD.Studio.EVENT_CALLBACK beatCallback;

    public static event Action<int, int, float> OnBeat;
    public static event Action<int, int, float> OnBeat2;
    public static event Action<int, int, float> OnBeat4;
    public static event Action<int, int, float> OnBeat8;

    private int globalBeatCount = 0;


    private MUSICS currentMusic;
    private AMBIENT currentAmb;

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
        if (musicAudioEvent.isValid())
        {
            musicAudioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicAudioEvent.release();
        }

        //Reset Beat Count
        globalBeatCount = 0;

        //STOP AMBIENT
        if (ambientAudioEvent.isValid())
        {
            ambientAudioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ambientAudioEvent.release();
        }

        if (sceneName == "MainMenu")
        {
            musicAudioEvent = RuntimeManager.CreateInstance(music_menu);
        }
        else if (sceneName == "Main")
        {
            switch (currentMusic)
            {
                case MUSICS.MUSIC_CAVE:
                    musicAudioEvent = RuntimeManager.CreateInstance(music_cave);
                    break;
                case MUSICS.MUSIC_LEVEL01:
                    musicAudioEvent = RuntimeManager.CreateInstance(music_level01);
                    break;
                case MUSICS.MUSIC_LEVEL02:
                    musicAudioEvent = RuntimeManager.CreateInstance(music_level02);
                    break;
                case MUSICS.MUSIC_LEVEL03:
                    musicAudioEvent = RuntimeManager.CreateInstance(music_level03);
                    break;
                case MUSICS.MUSIC_UFO:
                    musicAudioEvent = RuntimeManager.CreateInstance(music_UFO);
                    break;
                case MUSICS.MUSIC_BACKROOM:
                    musicAudioEvent = RuntimeManager.CreateInstance(music_backroom);
                    break;
                case MUSICS.NULL:
                    break;
            }

            switch (currentAmb)
            {
                case AMBIENT.NULL:
                    ambientAudioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    break;
                case AMBIENT.AMB_CITY:
                    ambientAudioEvent = RuntimeManager.CreateInstance(amb_city);
                    break;
                case AMBIENT.AMB_BEACH:
                    ambientAudioEvent = RuntimeManager.CreateInstance(amb_beach);
                    break;
                case AMBIENT.AMB_VOLCANO:
                    ambientAudioEvent = RuntimeManager.CreateInstance(amb_volcano);
                    break;
                case AMBIENT.AMB_SECRET:
                    ambientAudioEvent = RuntimeManager.CreateInstance(amb_secret);
                    break;
            }
        }

        // Rebind callback FMOD
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
        musicAudioEvent.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        if (thisHandle.IsAllocated)
            thisHandle.Free();

        thisHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
        musicAudioEvent.setUserData(GCHandle.ToIntPtr(thisHandle));

        musicAudioEvent.start();
    }

    private void FixedUpdate()
    {
        if (musicAudioEvent.isValid())
            musicAudioEvent.setParameterByName("Underwater", parameter);
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
            musicAudioEvent.setPaused(true);
        }
        else
        {
            isPaused = false;
            musicAudioEvent.setPaused(false);
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

                // Incrémente compteur global
                manager.globalBeatCount++;

                // Tous les 2 beats
                if (manager.globalBeatCount % 2 == 0)
                {
                    OnBeat2?.Invoke(beat.bar, beat.beat, beat.tempo);
                }

                // Tous les 4 beats
                if (manager.globalBeatCount % 4 == 0)
                {
                    OnBeat4?.Invoke(beat.bar, beat.beat, beat.tempo);
                }

                // Tous les 8 beats
                if (manager.globalBeatCount % 8 == 0)
                {
                    OnBeat8?.Invoke(beat.bar, beat.beat, beat.tempo);
                }
            }
        }

        return FMOD.RESULT.OK;
    }

    private void OnDestroy()
    {
        if (musicAudioEvent.isValid())
        {
            musicAudioEvent.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            musicAudioEvent.release();
        }

        if (thisHandle.IsAllocated)
            thisHandle.Free();
    }

    public void StopMusic()
    {
        if (musicAudioEvent.isValid())
        {
            musicAudioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicAudioEvent.release();
        }
    }

    public void PlayMusic(MUSICS newMusic)
    {
        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        if (currentMusic != newMusic)
            musicCoroutine = StartCoroutine(PlayMusicWithDelayCoroutine(newMusic, 1.5f));
    }

    private Coroutine musicCoroutine;

    private IEnumerator PlayMusicWithDelayCoroutine(MUSICS newMusic, float delay)
    {
        // 1. Fade out la musique actuelle
        if (musicAudioEvent.isValid())
        {
            musicAudioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicAudioEvent.release();
        }

        // 2. Attente
        yield return new WaitForSeconds(delay);

        //Reset Beat Count
        globalBeatCount = 0;

        // 3. Mise à jour de la musique courante
        currentMusic = newMusic;

        // 4. Création du nouvel EventInstance
        switch (currentMusic)
        {
            case MUSICS.MUSIC_MENU:
                musicAudioEvent = RuntimeManager.CreateInstance(music_menu);
                break;

            case MUSICS.MUSIC_CAVE:
                musicAudioEvent = RuntimeManager.CreateInstance(music_cave);
                break;

            case MUSICS.MUSIC_LEVEL01:
                musicAudioEvent = RuntimeManager.CreateInstance(music_level01);
                break;

            case MUSICS.MUSIC_LEVEL02:
                musicAudioEvent = RuntimeManager.CreateInstance(music_level02);
                break;

            case MUSICS.MUSIC_LEVEL03:
                musicAudioEvent = RuntimeManager.CreateInstance(music_level03);
                break;

            case MUSICS.MUSIC_UFO:
                musicAudioEvent = RuntimeManager.CreateInstance(music_UFO);
                break;

            case MUSICS.MUSIC_BACKROOM:
                musicAudioEvent = RuntimeManager.CreateInstance(music_backroom);
                break;
        }

        // 5. Rebind du callback FMOD (IMPORTANT)
        beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
        musicAudioEvent.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT);

        if (thisHandle.IsAllocated)
            thisHandle.Free();

        thisHandle = GCHandle.Alloc(this, GCHandleType.Pinned);
        musicAudioEvent.setUserData(GCHandle.ToIntPtr(thisHandle));

        // 6. Play
        musicAudioEvent.start();
    }

    private Coroutine ambientCoroutine;

    public void PlayAmbient(AMBIENT newAmbient)
    {
        if (ambientCoroutine != null)
            StopCoroutine(ambientCoroutine);

        if (currentAmb != newAmbient)
            ambientCoroutine = StartCoroutine(PlayAmbientWithDelayCoroutine(newAmbient, 1.5f));
    }

    private IEnumerator PlayAmbientWithDelayCoroutine(AMBIENT newAmbient, float delay)
    {
        // 1. Fade out la musique actuelle
        if (ambientAudioEvent.isValid())
        {
            ambientAudioEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ambientAudioEvent.release();
        }

        // 2. Attente
        yield return new WaitForSeconds(delay);

        // 3. Mise à jour de la musique courante
        currentAmb = newAmbient;

        // 4. Création du nouvel EventInstance
        switch (currentAmb)
        {
            case AMBIENT.NULL:
                yield break;
            case AMBIENT.AMB_CITY:
                ambientAudioEvent = RuntimeManager.CreateInstance(amb_city);
                break;
            case AMBIENT.AMB_BEACH:
                ambientAudioEvent = RuntimeManager.CreateInstance(amb_beach);
                break;
            case AMBIENT.AMB_VOLCANO:
                ambientAudioEvent = RuntimeManager.CreateInstance(amb_volcano);
                break;
            case AMBIENT.AMB_SECRET:
                ambientAudioEvent = RuntimeManager.CreateInstance(amb_secret);
                break;
        }

        // 5. Play
        ambientAudioEvent.start();
    }
}

