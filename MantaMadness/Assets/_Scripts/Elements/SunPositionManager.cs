using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SunState
{
    Sun,
    Moon,
    Why,
}

public class SunPositionManager : MonoBehaviour, IDataPersistence
{
    public static SunPositionManager instance;

    [SerializeField] private Transform shoresPos;
    [SerializeField] private Transform volcanoPos;
    [SerializeField] private Transform cityPos;

    [SerializeField] private UnityEngine.GameObject sunVisual;
    [SerializeField] private Renderer[] sunFaceRenderer;
    [SerializeField] private UnityEngine.GameObject[] sunStateVisual;
    [SerializeField] private UnityEngine.GameObject[] moonStateVisual;
    [SerializeField] private UnityEngine.GameObject[] whystateVisual;

    [SerializeField] private Material sunMat;
    [SerializeField] private Material moonMat;

    private SunState currentState;
    private Vector3 originalScale;

    private void Awake()
    {
        if(instance == null)
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
        originalScale = sunVisual.transform.localScale;
        SetSunPosition(data.weatherCondition);
    }

    public void SaveData(ref GameData data)
    {
        //RIEN
    }

    public void SetSunPosition(WeatherType weather)
    {
        if(sunSwitchRoutine == null)
        {
            switch (weather)
            {
                case WeatherType.Shores:
                    sunSwitchRoutine = StartCoroutine(SunSwitchPosition(shoresPos, SunState.Sun));
                    break;
                case WeatherType.Vulcano:
                    sunSwitchRoutine = StartCoroutine(SunSwitchPosition(volcanoPos, SunState.Sun));
                    break;
                case WeatherType.City:
                    sunSwitchRoutine = StartCoroutine(SunSwitchPosition(cityPos, SunState.Moon));
                    break;
                case WeatherType.AlienField:
                    sunSwitchRoutine = StartCoroutine(SunSwitchPosition(cityPos, SunState.Moon));
                    break;
                default:
                    sunSwitchRoutine = StartCoroutine(SunSwitchPosition(shoresPos, SunState.Sun));
                    break;
            }
        }
    }

    private Coroutine sunSwitchRoutine = null;
    private IEnumerator SunSwitchPosition(Transform swichPosition, SunState state)
    {

        if(SceneManager.GetActiveScene() == SceneManager.GetSceneByName("MainMenu"))
        {
            if (state == SunState.Sun)
            {
                foreach (UnityEngine.GameObject visual in sunStateVisual)
                {
                    visual.SetActive(true);
                }

                foreach (UnityEngine.GameObject visual in moonStateVisual)
                {
                    visual.SetActive(false);
                }

                foreach (Renderer renderer in sunFaceRenderer)
                {
                    renderer.material = sunMat;
                }
            }
            else
            {
                foreach (UnityEngine.GameObject visual in sunStateVisual)
                {
                    visual.SetActive(false);
                }

                foreach (UnityEngine.GameObject visual in moonStateVisual)
                {
                    visual.SetActive(true);
                }

                foreach (Renderer renderer in sunFaceRenderer)
                {
                    renderer.material = moonMat;
                }
            }
        }
        else
        {
            sunVisual.transform.DOScale(Vector3.one, 1.5f).SetEase(Ease.InOutQuad);
            yield return new WaitForSeconds(2f);

            if (Game.Instance.why == false)
            {
                if (state == SunState.Sun)
                {
                    foreach (UnityEngine.GameObject visual in sunStateVisual)
                    {
                        visual.SetActive(true);
                    }

                    foreach (UnityEngine.GameObject visual in moonStateVisual)
                    {
                        visual.SetActive(false);
                    }

                    foreach (Renderer renderer in sunFaceRenderer)
                    {
                        renderer.material = sunMat;
                    }
                }
                else
                {
                    foreach (UnityEngine.GameObject visual in sunStateVisual)
                    {
                        visual.SetActive(false);
                    }

                    foreach (UnityEngine.GameObject visual in moonStateVisual)
                    {
                        visual.SetActive(true);
                    }

                    foreach (Renderer renderer in sunFaceRenderer)
                    {
                        renderer.material = moonMat;
                    }
                }
            }


            sunVisual.transform.position = swichPosition.position;
            sunVisual.transform.DOScale(originalScale, 1.5f).SetEase(Ease.InOutQuad);
            sunSwitchRoutine = null;
        }

    }

    public void SetSunStateOnload(WeatherType type)
    {
        switch (type)
        {
            case WeatherType.Vulcano:
                SetSunState(SunState.Sun);
                break;
            case WeatherType.City:
                SetSunState(SunState.Moon);
                break;
            case WeatherType.Shores:
                SetSunState(SunState.Sun);
                break;
            case WeatherType.Why:
                SetSunState(SunState.Why);
                break;
            default:
                SetSunState(SunState.Sun);
                Debug.Log("Default Case quelqu'un ?");
                break;
        }

    }

    public void SetSunState(SunState state)
    {
        if (state == SunState.Sun)
        {
            foreach (UnityEngine.GameObject visual in sunStateVisual)
            {
                visual.SetActive(true);
            }

            foreach (UnityEngine.GameObject visual in moonStateVisual)
            {
                visual.SetActive(false);
            }

            foreach (Renderer renderer in sunFaceRenderer)
            {
                renderer.material = sunMat;
            }
        }
        else if (state == SunState.Moon)
        {
            foreach (UnityEngine.GameObject visual in sunStateVisual)
            {
                visual.SetActive(false);
            }

            foreach (UnityEngine.GameObject visual in moonStateVisual)
            {
                visual.SetActive(true);
            }

            foreach (Renderer renderer in sunFaceRenderer)
            {
                renderer.material = moonMat;
            }
        }
        else if(state == SunState.Why)
        {
            foreach (UnityEngine.GameObject visual in sunStateVisual)
            {
                visual.SetActive(true);
            }

            foreach (UnityEngine.GameObject visual in moonStateVisual)
            {
                visual.SetActive(false);
            }

            foreach (Renderer renderer in sunFaceRenderer)
            {
                renderer.material = sunMat;
            }

            foreach (UnityEngine.GameObject visual in whystateVisual)
            {
                visual.SetActive(false);
            }
        }
    }


}
