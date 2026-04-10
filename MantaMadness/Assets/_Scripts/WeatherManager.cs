using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.LookDev;

public enum WeatherType
{
    Shores,
    City,
    Vulcano,
    MountainTemple,
}

public enum FogState
{
    disabled,
    enabled,
    specialEnabled,
}

[System.Serializable]
public struct weatherColor
{
    public WeatherType type;
    [ColorUsage(showAlpha: true, hdr: true)]
    public Color color;
    [ColorUsage(showAlpha: true, hdr: true)]
    public Color fogColorClose;
    [ColorUsage(showAlpha: true, hdr: true)]
    public Color fogColorFar;
    public MUSICS music;
}



public class WeatherManager : MonoBehaviour, IDataPersistence
{
    public static WeatherManager instance;

    [SerializeField] private weatherColor[] weatherConditions;

    public WeatherType currentWeather;
    private FogState currentFogState;

    [SerializeField] private Ease ease;
    [SerializeField] private float easeDuration;

    [SerializeField] private FogTypeClass[] fogs;


    // But de ce script > lorsque le joueur interagis avec une zone "SwitchWeatherCondition" dans le world
    // > le ciel va blend sa couleur vers la couleur du nouveau weatherColor
    // > La condition actuel du ciel est suavegardé dans le data (la data save un weathertype, comme ca au start du weather manager > le script load le ciel correspondant au weather type sauvegarder.

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void LoadData(GameData data)
    {
        StartCoroutine(LoadDataDelay(data));
    }

    public IEnumerator LoadDataDelay(GameData data)
    {
        yield return new WaitForSeconds(0.1f);
        SetWeatherOnLoad(data.weatherCondition);
        currentWeather = data.weatherCondition;
        currentFogState = data.fogState;
    }

    public void SaveData(ref GameData data)
    {
        data.weatherCondition = currentWeather;
        data.fogState = currentFogState;
    }

    public void SetWeatherOnLoad(WeatherType newWeather)
    {
        if (currentWeather == newWeather)
            return;

        foreach (weatherColor condition in weatherConditions)
        {
            if (condition.type == newWeather)
            {
                Material sky = RenderSettings.skybox;

                sky.DOColor(condition.color, "_Tint", easeDuration).SetEase(ease).OnUpdate(() => DynamicGI.UpdateEnvironment());

                currentWeather = newWeather;

                foreach (FogTypeClass fog in fogs)
                {
                    if (fog.type == FogTypeClass.FogType.Close || fog.type == FogTypeClass.FogType.Special)
                    {
                        fog.fogMat.DOColor(condition.fogColorClose, "_FogColor", easeDuration).SetEase(ease).OnUpdate(() => DynamicGI.UpdateEnvironment());
                    }
                    else
                    {
                        fog.fogMat.DOColor(condition.fogColorFar, "_FogColor", easeDuration).SetEase(ease).OnUpdate(() => DynamicGI.UpdateEnvironment());
                    }
                }
            }
        }
    }

    public void SetNewWeather(WeatherType newWeather)
    {
        if (currentWeather == newWeather)
            return;

        Debug.Log("Frr wsh pk srx?");
        foreach (weatherColor condition in weatherConditions)
        {
            if (condition.type == newWeather)
            {
                Material sky = RenderSettings.skybox;

                sky.DOColor(condition.color, "_Tint", easeDuration).SetEase(ease).OnUpdate(() => DynamicGI.UpdateEnvironment());

                currentWeather = newWeather;

                foreach(FogTypeClass fog in fogs)
                {
                    if(fog.type == FogTypeClass.FogType.Close || fog.type == FogTypeClass.FogType.Special)
                    {
                        fog.fogMat.DOColor(condition.fogColorClose, "_FogColor", easeDuration).SetEase(ease).OnUpdate(() => DynamicGI.UpdateEnvironment());
                    }
                    else
                    {
                        fog.fogMat.DOColor(condition.fogColorFar, "_FogColor", easeDuration).SetEase(ease).OnUpdate(() => DynamicGI.UpdateEnvironment());
                    }
                }

                if (condition.music != MUSICS.NULL)
                {
                    MusicManager.Instance.PlayMusic(condition.music);
                }
            }
        }

        Debug.Log("Frr wsh pk?");
        if (SunPositionManager.instance != null)
        {
            SunPositionManager.instance.SetSunPosition(newWeather);
        }
    }

    public void UpdateFog(FogState state, WeatherType specialType)
    {
        switch (state)
        {
            case FogState.disabled:
                foreach (FogTypeClass fog in fogs)
                {
                    fog.gameObject.SetActive(false);
                }
                break;
            case FogState.enabled:
                foreach (FogTypeClass fog in fogs)
                {
                    if(fog.type != FogTypeClass.FogType.Special)
                    {
                        fog.gameObject.SetActive(true);
                    }
                    else
                    {
                        fog.gameObject.SetActive(false);
                    }
                }
                break;
            case FogState.specialEnabled:
                foreach (FogTypeClass fog in fogs)
                {
                    fog.gameObject.SetActive(true);
                }
                SetNewWeather(specialType);
                break;
        }
        currentFogState = state;
    }

    public void DebugSwitchCondition()
    {
        int enumLength = System.Enum.GetValues(typeof(WeatherType)).Length;
        int next = ((int)currentWeather + 1) % enumLength;
        SetNewWeather((WeatherType)next);
    }
}
