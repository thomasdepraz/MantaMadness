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
    }

    public bool Respawn(out Transform respawn)
    {
        respawn = null;
        if (raceManager.TryGetRespawn(out respawn))
            return true;

        return false;
    }
}
