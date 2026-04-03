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

    private WeatherType currentWeather;
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
        SetNewWeather(data.weatherCondition);
        currentWeather = data.weatherCondition;
        currentFogState = data.fogState;
    }

    public void SaveData(ref GameData data)
    {
        data.weatherCondition = currentWeather;
        data.fogState = currentFogState;
    }

    public void SetNewWeather(WeatherType newWeather)
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


    //Maintenant nouvelle logique qui rejoint la precedente
    // L'idée c'est qu'en gros, on get le mat de tout les fog layer. donc faut pouvoir differencier les fogs selon leur type (Fog near / fog far > il on un mat différent)
    //Ensuite lors de l'update du sky, il faut que les mat des fog se set selon le weatherColor, ca signifie qu'il faut rajouter 2 parametres au weather color, soit fogColorNear et fogColorFar;
    //Puis lors du changement de biome, il faut faire blend les materiaux de chaque fog selon la valeur de weathercolor qui correspond au type du fog
    // Donc en gros, chaque fog doit aussi avoir un script FOG avec un fogtype et selon le fogtype, on applique la bonne couleur

    //2e, un systeme pour desactiver le fog si on est dans une zone qui ne l'utilise pas. la facon la plus simple c'est:
    // Lié au TP, lorsque le perso tp > on check si le tp est de type "Enable" ou "Disable" ou "Null". Si enable > active le fog / Si disable desactive le fog
    // Aussi il faut pour register tout ca, créer un booléen dans la gameData qu'on appelera "fogEnabled" et il faut l'enregistrer au moment de save data.

    public void DebugSwitchCondition()
    {
        int enumLength = System.Enum.GetValues(typeof(WeatherType)).Length;
        int next = ((int)currentWeather + 1) % enumLength;
        SetNewWeather((WeatherType)next);
    }
}
