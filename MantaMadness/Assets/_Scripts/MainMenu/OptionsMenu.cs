using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections.Generic;
using TMPro;
using UnityEditor;

public class OptionsMenu : MonoBehaviour
{
    public static OptionsMenu instance;

    [Header("All visuals")]
    [SerializeField] private GameObject[] visuals;

    [Header("UI")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Toggle invertCamToggle;

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Navigation")]
    [SerializeField] public IOptionItem[] optionItems;


    public enum OptionsContext
    {
        MainMenu,
        PauseMenu
    }

    private OptionsContext currentContext;

    public void OpenFromMainMenu()
    {
        currentContext = OptionsContext.MainMenu;
        Open();
    }

    public void OpenFromPauseMenu()
    {
        currentContext = OptionsContext.PauseMenu;
        Open();
    }

    public bool HandleCancel()
    {
        if (optionItems[currentIndex].IsEditing)
        {
            optionItems[currentIndex].Cancel();
            return true; // consommé
        }

        return false; // pas consommé
    }

    private bool blockSubmit;


    private int currentIndex;


    private Resolution[] resolutions;

    private const string MASTER_VOL = "MasterVolume";
    private const string MUSIC_VOL = "MusicVolume";
    private const string SFX_VOL = "SFXVolume";

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

        InitResolutions();
        InitScreenMode();
        InitInvertAxis();
        //LoadAudioSettings();
    }


    private void Start()
    {
        ToggleVisuals(false);
    }

    private bool isInitializing;

    private void InitResolutions()
    {
        isInitializing = true;

        resolutionDropdown.ClearOptions();

        List<Resolution> filteredResolutions = new List<Resolution>();
        List<string> options = new List<string>();

        foreach (Resolution res in Screen.resolutions)
        {
            bool alreadyExists = false;

            foreach (Resolution filtered in filteredResolutions)
            {
                if (filtered.width == res.width && filtered.height == res.height)
                {
                    alreadyExists = true;
                    break;
                }
            }

            if (!alreadyExists)
            {
                filteredResolutions.Add(res);
                options.Add($"{res.width} x {res.height}");
            }
        }

        resolutions = filteredResolutions.ToArray();

        resolutionDropdown.AddOptions(options);

        int currentResolutionIndex = resolutions.Length - 1;

        int savedIndex = PlayerPrefs.GetInt("resolutionIndex", currentResolutionIndex);
        resolutionDropdown.value = savedIndex;
        resolutionDropdown.RefreshShownValue();

        resolutionDropdown.onValueChanged.RemoveAllListeners();
        resolutionDropdown.onValueChanged.AddListener(SetResolution);

        isInitializing = false;
    }


    public void SetResolution(int index)
    {
        if (isInitializing)
            return;

        Resolution res = resolutions[index];

        Screen.SetResolution(
            res.width,
            res.height,
            Screen.fullScreenMode
        );

        PlayerPrefs.SetInt("resolutionIndex", index);
    }

    private void InitScreenMode()
    {
        bool fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;

        fullscreenToggle.isOn = fullscreen;
        Screen.fullScreen = fullscreen;

        fullscreenToggle.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    public void SetScreenMode(int index)
    {
        bool fullscreen = index == 0;
        Screen.fullScreen = fullscreen;
        PlayerPrefs.SetInt("fullscreen", fullscreen ? 1 : 0);
    }

    public void InitInvertAxis()
    {
        bool invert = PlayerPrefs.GetInt("invertAxis", 0) == 1;

        invertCamToggle.isOn = invert;

        invertCamToggle.onValueChanged.RemoveAllListeners();
        invertCamToggle.onValueChanged.AddListener(SetInvertAxis);
    }

    public void SetInvertAxis(bool invert)
    {
        PlayerPrefs.SetInt("invertAxis", invert ? 1 : 0);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("fullscreen", isFullscreen ? 1 : 0);
    }

    public void Enable()
    {
        ToggleVisuals(true);
    }

    public void CloseFromPause()
    {
        PlayerPrefs.Save();
        ToggleVisuals(false);
        gameObject.SetActive(false);

        PauseMenu.instance.ReturnFromOptions();
    }

    public void CloseFromMainMenu()
    {
        PlayerPrefs.Save();
        ToggleVisuals(false);
        gameObject.SetActive(false);

        MainMenu.instance.State = MainMenu.MainMenuState.DEFAULT;
    }

    public void ToggleVisuals(bool toggleValue)
    {
        if (toggleValue)
        {
            foreach(GameObject visual in visuals)
            {
                visual.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject visual in visuals)
            {
                visual.SetActive(false);
            }
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);

        Enable();
        currentIndex = 0;

        foreach (var option in optionItems)
            option.ForceExitEdit();

        optionItems[currentIndex].Select();

        blockSubmit = true;
        StartCoroutine(UnblockSubmitNextFrame());
    }

    private System.Collections.IEnumerator UnblockSubmitNextFrame()
    {
        yield return null;
        blockSubmit = false;
    }


    public void MoveUp()
    {
        if (optionItems[currentIndex].IsEditing)
        {
            optionItems[currentIndex].OnNavigateUp();
            return;
        }

        optionItems[currentIndex].Deselect();
        currentIndex = Mathf.Max(0, currentIndex - 1);
        optionItems[currentIndex].Select();
    }

    public void MoveDown()
    {
        if (optionItems[currentIndex].IsEditing)
        {
            optionItems[currentIndex].OnNavigateDown();
            return;
        }

        optionItems[currentIndex].Deselect();
        currentIndex = Mathf.Min(optionItems.Length - 1, currentIndex + 1);
        optionItems[currentIndex].Select();
    }

    public void MoveLeft()
    {
        optionItems[currentIndex].Decrease();
    }

    public void MoveRight()
    {
        optionItems[currentIndex].Increase();
    }

    public void Submit()
    {
        if (blockSubmit) return;
        optionItems[currentIndex].Submit();
    }

    public void Cancel()
    {
        if (HandleCancel())
            return;

        RequestClose();
    }

    public void RequestClose()
    {
        if (currentContext == OptionsContext.MainMenu)
            CloseFromMainMenu();
    }

}

