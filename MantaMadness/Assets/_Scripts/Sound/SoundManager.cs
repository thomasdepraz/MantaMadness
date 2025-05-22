using UnityEngine;
using System;

public enum SoundType
{
    BOOST,
    JUMP
}

public enum Music
{
    THEME_001,
    THEME_002
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundList[] soundList;
    [SerializeField] private MusicList[] musicList;
    private AudioSource audioSource;

    public static SoundManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayOneShotSound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = Instance.soundList[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        Instance.audioSource.PlayOneShot(randomClip, volume);
    }

    public void PlayMusic(Music music, float volume = 0.3f)
    {
        AudioClip clip = Instance.musicList[(int)music].music;
        Instance.audioSource.resource = clip;
        Instance.audioSource.volume = volume;
        Instance.audioSource.loop = true;
        Instance.audioSource.Play();
    }

#if UNITY_EDITOR
    [ContextMenu("Resize")]
    public void Resize()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundList, names.Length);
        for (int i = 0; i < soundList.Length; i++)
        {
            soundList[i].name = names[i];
        }

        string[] musicNames = Enum.GetNames(typeof(Music));
        Array.Resize(ref musicList, musicNames.Length);
        for (int i = 0; i < musicList.Length; i++)
        {
            musicList[i].name = musicNames[i];
        }
    }
#endif
}

[Serializable]
public struct SoundList
{
    public AudioClip[] Sounds { get => sounds; }
    [SerializeField] public string name;
    [SerializeField] private AudioClip[] sounds;
}

[Serializable]
public struct MusicList
{
    [SerializeField] public string name;
    [SerializeField] public AudioClip music;
}
