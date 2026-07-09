using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum WeatherType
{
    Shores,
    City,
    Vulcano,
    MountainTemple,
    Null,
    SunAltar,
    Sewer,
    AlienField,
    LavaHeart,
    Backroom,
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
    public FogState fogState;
    public MUSICS music;
    public AMBIENT ambient;
}



public class WeatherManager : MonoBehaviour, IDataPersistence
{
    public static WeatherManager instance;

    [SerializeField] private weatherColor[] weatherConditions;

    public WeatherType currentWeather;
    public FogState currentFogState;

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
        if(SceneManager.GetActiveScene() == SceneManager.GetSceneByName("MainMenu"))
        {
            SetWeatherOnLoad(data.mainMenuWeatherCondition);
        }
        else
        {
            SetWeatherOnLoad(data.weatherCondition);
        }

    }

    public void SaveData(ref GameData data)
    {
        data.weatherCondition = currentWeather;
        data.fogState = currentFogState;

        //Save Weather State if its a valid Main menu State.
        switch (currentWeather)
        {
            case WeatherType.Vulcano:
                data.mainMenuWeatherCondition = currentWeather;
                break;
            case WeatherType.City:
                data.mainMenuWeatherCondition = currentWeather;
                break;
            case WeatherType.Shores:
                data.mainMenuWeatherCondition = currentWeather;
                break;
        }
    }

    public void SetWeatherOnLoad(WeatherType newWeather)
    {
        if (newWeather == WeatherType.Null)
        {
            foreach (weatherColor condition in weatherConditions)
            {
                if (condition.type == WeatherType.Null)
                {
                    UpdateFog(condition.fogState);
                    currentWeather = WeatherType.Null;
                }
            }
            return;
        }

        foreach (weatherColor condition in weatherConditions)
        {


            if (condition.type == newWeather)
            {
                Material sky = RenderSettings.skybox;

                sky.DOColor(condition.color, "_Tint", easeDuration).SetEase(ease).OnUpdate(() => DynamicGI.UpdateEnvironment());

                currentWeather = newWeather;

                UpdateFog(condition.fogState);

                if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("MainMenu"))
                {
                    SunPositionManager.instance.SetSunStateOnload(DataPersistenceManager.Instance.gameData.mainMenuWeatherCondition);
                    return;
                }


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

        if (SunPositionManager.instance != null)
        {
            SunPositionManager.instance.SetSunPosition(newWeather);
        }

    }

    public void SetNewWeather(WeatherType newWeather)
    {
        if (currentWeather == newWeather)
           return;

        if (newWeather == WeatherType.Null)
        {
            foreach (weatherColor condition in weatherConditions)
            {
                if (condition.type == WeatherType.Null)
                {
                    UpdateFog(condition.fogState);
                    currentWeather = WeatherType.Null;
                }
            }
            return;
        }

        foreach (weatherColor condition in weatherConditions)
        {
            if (condition.type == newWeather)
            {
                Material sky = RenderSettings.skybox;

                sky.DOColor(condition.color, "_Tint", easeDuration).SetEase(ease).OnUpdate(() => DynamicGI.UpdateEnvironment());

                currentWeather = newWeather;

                UpdateFog(condition.fogState);

                foreach (FogTypeClass fog in fogs)
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

                if(condition.ambient != AMBIENT.NULL)
                {
                    MusicManager.Instance.PlayAmbient(condition.ambient);
                }
            }
        }

        if (SunPositionManager.instance != null)
        {
            SunPositionManager.instance.SetSunPosition(newWeather);
        }


        //Set weather Conditions in save
        switch (currentWeather)
        {
            case WeatherType.Vulcano:
                DataPersistenceManager.Instance.gameData.mainMenuWeatherCondition = currentWeather;
                break;
            case WeatherType.City:
                DataPersistenceManager.Instance.gameData.mainMenuWeatherCondition = currentWeather;
                break;
            case WeatherType.Shores:
                DataPersistenceManager.Instance.gameData.mainMenuWeatherCondition = currentWeather;
                break;
            case WeatherType.AlienField:
                DataPersistenceManager.Instance.gameData.mainMenuWeatherCondition = currentWeather;
                break;
        }
    }

    public void UpdateFog(FogState state)
    {
        Debug.Log("Fog State is " + state);
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
                    if (fog == null) return;

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

    public void DebugFogCondition()
    {
        int enumLength = System.Enum.GetValues(typeof(FogState)).Length;
        int next = ((int)currentFogState + 1) % enumLength;
        UpdateFog((FogState)next);
    }

}
