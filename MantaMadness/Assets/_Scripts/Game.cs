using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Game : MonoBehaviour
{
    public static Game Instance;
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

        Application.targetFrameRate = 60;
    }

    public ScreenEffectData screenEffectData;

    [HideInInspector] public SimpleController player;
    public RaceManager raceManager = new RaceManager();
    CameraManager cameraManager = CameraManager.Instance;
    public SaveManager saveManager;

    [HideInInspector] public Vector3 m_SpawnPosition;
    [HideInInspector] public Quaternion m_SpawnRotation;

    public void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SimpleController>();

        //Toggle screen effects
        List<ScriptableRendererFeature> scriptableRendererFeatures = RenderFeatureUtility.GetRenderFeatures();
        foreach(var effect in screenEffectData.ScreenEffects)
        {
            ScriptableRendererFeature scriptableRendererFeature = RenderFeatureUtility.GetFeature(scriptableRendererFeatures, effect.featureName);
            scriptableRendererFeature.SetActive(effect.isActive);
        }

        // PLay level Music
        //SoundManager.PlayMusic(Music.THEME_001);

        m_SpawnPosition = player.transform.position;
        m_SpawnRotation = player.transform.rotation;
    }

    public void Respawn(out Vector3 position, out Quaternion rotation)
    {
        if (raceManager.TryGetRespawn(out Transform respawn))
        {
            position = respawn.position;
            rotation = respawn.rotation;

            player.ForcePosition(position, rotation);
            return;
        }
        
        position = m_SpawnPosition;
        rotation = m_SpawnRotation;

        SoundManager.Instance.PlayOneShotSound(SoundType.SPLASH);
        player.ForcePosition(position, rotation);
    }

    public void SetRespawnTransform(Transform respawnTransform)
    {
        m_SpawnPosition = respawnTransform.position;
        m_SpawnRotation = respawnTransform.rotation;
    }
}
