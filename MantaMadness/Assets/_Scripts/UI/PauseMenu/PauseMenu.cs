using DG.Tweening;
using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static MainMenu;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu instance;

    [Header("References")]
    public InputManager inputs;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject pauseMenuBackground;
    [SerializeField] private GameObject[] pauseMenuButtons;

    [SerializeField] private OptionsMenu optionsMenu;
    [SerializeField] public ConfirmUnstuck unstuckMenu;

    [Header("Sound parameters")]
    [SerializeField] private EventReference submitSound;
    [SerializeField] private EventReference navigateSound;

    public bool isPaused = false;
    private bool ignoreNextInput = false;

    private int currentIndex;

    public enum PauseMenuState
    {
        DEFAULT,
        OPTIONS,
        UNSTUCK,
        NULL
    }

    private PauseMenuState state;
    private PauseMenuState previousState;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        if (inputs == null)
            inputs = InputManager.Instance;

        previousState = PauseMenuState.NULL;
        state = PauseMenuState.NULL;

        pauseMenuUI.SetActive(false);
        pauseMenuBackground.SetActive(false);
        currentIndex = 0;


        inputs.uiMoveDown.action.performed += IncreaseIndex;
        inputs.uiMoveUp.action.performed += DecreaseIndex;
        inputs.uiSubmit.action.performed += Submit;
        inputs.uiCancel.action.performed += Cancel;
        inputs.uiPause.action.performed += StartPause;
        inputs.uiMoveLeft.action.performed += OnMoveLeft;
        inputs.uiMoveRight.action.performed += OnMoveRight;
        //inputs.pause.action.performed += StartPause;
    }

    private void OnDisable()
    {
        inputs.uiMoveDown.action.performed -= IncreaseIndex;
        inputs.uiMoveUp.action.performed -= DecreaseIndex;
        inputs.uiSubmit.action.performed -= Submit;
        inputs.uiCancel.action.performed -= Cancel;
        inputs.uiPause.action.performed -= StartPause;
        inputs.uiMoveLeft.action.performed -= OnMoveLeft;
        inputs.uiMoveRight.action.performed -= OnMoveRight;
        //inputs.pause.action.performed -= StartPause;
    }

    public void StartPause(InputAction.CallbackContext ctx)
    {
        if (isPaused && state == PauseMenuState.OPTIONS)
        {
            optionsMenu.CloseFromPause();
            return;
        }

        if (!isPaused)
        {
            Pause();
            return;
        }

        if (isPaused && state == PauseMenuState.DEFAULT)
        {
            Resume();
        }
    }

    #region State Management
    private void UpdateState()
    {
        if (previousState != PauseMenuState.NULL)
            ExitState(previousState);

        EnterState(state);
    }

    private void EnterState(PauseMenuState newState)
    {
        switch (newState)
        {
            case PauseMenuState.DEFAULT:
                pauseMenuUI.SetActive(true);
                UpdateButtons();
                break;

            case PauseMenuState.OPTIONS:
                pauseMenuUI.SetActive(false);
                optionsMenu.OpenFromPauseMenu();
                break;

            case PauseMenuState.UNSTUCK:
                pauseMenuUI.SetActive(false);
                unstuckMenu.Open();
                break;

        }
    }

    private void ExitState(PauseMenuState oldState)
    {
        if (oldState == PauseMenuState.DEFAULT)
            pauseMenuUI.SetActive(false);

        if (oldState == PauseMenuState.OPTIONS)
            optionsMenu.CloseFromPause();
    }
    #endregion

    #region Inputs
    private void IncreaseIndex(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        if (state == PauseMenuState.OPTIONS)
        {
            optionsMenu.MoveDown();
            return;
        }


        if (state == PauseMenuState.UNSTUCK)
        {
            unstuckMenu.MoveDown();
            return;
        }


        if (state != PauseMenuState.DEFAULT || ignoreNextInput) return;

        PlaySound(navigateSound);
        currentIndex = (currentIndex + 1) % pauseMenuButtons.Length;
        UpdateButtons();
    }

    private void DecreaseIndex(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        if (state == PauseMenuState.OPTIONS)
        {
            optionsMenu.MoveUp();
            return;
        }

        if (state == PauseMenuState.UNSTUCK)
        {
            unstuckMenu.MoveUp();
            return;
        }

        if (state != PauseMenuState.DEFAULT || ignoreNextInput) return;

        PlaySound(navigateSound);
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = pauseMenuButtons.Length - 1;

        UpdateButtons();
    }

    private void Submit(InputAction.CallbackContext ctx)
    {
        if (!isPaused || ignoreNextInput) return;

        PlaySound(submitSound);

        if (state == PauseMenuState.OPTIONS)
        {
            optionsMenu.Submit();
            return;
        }

        else if (state == PauseMenuState.UNSTUCK)
        {
            unstuckMenu.Submit();
            return;
        }

        if (state != PauseMenuState.DEFAULT) return;

        switch (currentIndex)
        {
            case 0: Resume(); break;
            case 1: Respawn(); break;
            case 2: OpenOptions(); break;
            case 3: OpenUnstuck(); break;
            case 4: LoadMainMenu(); break;
            case 5: QuitGame(); break;
        }
    }
    private void OpenOptions()
    {
        previousState = state;
        state = PauseMenuState.OPTIONS;
        UpdateState();
    }

    private void Cancel(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        if (state == PauseMenuState.OPTIONS)
        {
            if (optionsMenu.HandleCancel())
                return;

            optionsMenu.CloseFromPause();
            return;
        }

        if(state == PauseMenuState.UNSTUCK)
        {
            unstuckMenu.Cancel();
            return;
        }
        Resume();
    }
    #endregion

    #region Actions
    private void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;
        DOTween.PauseAll();

        pauseMenuBackground.SetActive(true);
        UIManager.Instance.ToggleBaseInterface(false);
        UIManager.Instance.dialogInteractDisplay.ToggleInterface(false);
        previousState = state;
        state = PauseMenuState.DEFAULT;
        UpdateState();

        StartCoroutine(SwitchToUINextFrame());
    }

    private IEnumerator SwitchToUINextFrame()
    {
        yield return null; // attendre la fin de la frame
        inputs.EnableUI();
    }

    public void Resume()
    {
        if (!isPaused) return;

        Time.timeScale = 1f;
        DOTween.PlayAll();
        isPaused = false;

        pauseMenuBackground.SetActive(false);
        UIManager.Instance.ToggleBaseInterface(true);
        previousState = state;
        state = PauseMenuState.NULL;
        UpdateState();

        StartCoroutine(SwitchToGameplayNextFrame());
    }

    private IEnumerator SwitchToGameplayNextFrame()
    {
        yield return null;
        inputs.EnableGameplay();
    }

    private void Respawn()
    {
        Resume();
        Game.Instance.Respawn(out Game.Instance.m_SpawnPosition, out Game.Instance.m_SpawnRotation);
    }

    private void LoadMainMenu()
    {
        Time.timeScale = 1f;
        DOTween.PlayAll();
        SceneManager.LoadScene("MainMenu");
        DataPersistenceManager.Instance.SaveGame();
    }

    private void QuitGame()
    {
        Application.Quit();
    }
    #endregion

    #region UI
    private void UpdateButtons()
    {
        for (int i = 0; i < pauseMenuButtons.Length; i++)
        {
            if (i == currentIndex)
                pauseMenuButtons[i].GetComponent<PauseMenuButton>().EnableButton();
            else
                pauseMenuButtons[i].GetComponent<PauseMenuButton>().ResetButton();
        }
    }
    #endregion

    public void PlaySound(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound);
    }
    public void ReturnFromOptions()
    {
        previousState = state;
        state = PauseMenuState.DEFAULT;
        UpdateState();

        ignoreNextInput = true;
        StartCoroutine(UnignoreNextFrame());
    }

    private IEnumerator UnignoreNextFrame()
    {
        yield return null;
        ignoreNextInput = false;
    }

    private void OnMoveLeft(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        if (state == PauseMenuState.OPTIONS)
        {
            optionsMenu.MoveLeft();
        }
    }

    private void OnMoveRight(InputAction.CallbackContext ctx)
    {
        if (!isPaused) return;

        if (state == PauseMenuState.OPTIONS)
        {
            optionsMenu.MoveRight();
        }
    }

    private void OpenUnstuck()
    {
        previousState = state;
        state = PauseMenuState.UNSTUCK;
        UpdateState();
    }

    public void CloseUnstuck()
    {
        previousState = state;
        state = PauseMenuState.DEFAULT;
        UpdateState();
    }

}