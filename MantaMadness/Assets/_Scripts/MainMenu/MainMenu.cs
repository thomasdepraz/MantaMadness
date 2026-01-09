using System.Linq;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using static UnityEngine.CullingGroup;
using FMODUnity;
using System.Collections;

public class MainMenu : MonoBehaviour
{
    //Script pour gerer le main menu

    //IL doit  servir à :
    //Créer une nouvelle partie et la commencer
    //Continué une partie en cours grace à un save file (prévisualistion de la save)
    //Acceder au options
    //+ bouton wishlist pour la demo
    // Quit game

    public static MainMenu instance;

    public InputManager inputs;
    [SerializeField] public OptionsMenu options;

    [Header("Sound parameters")]
    [SerializeField] public EventReference submitSound;
    [SerializeField] public EventReference navigateSound;
    [SerializeField] public EventReference mainMenuTweakSound;
    [SerializeField] public EventReference startGameSound;

    public enum MainMenuState
    {
        DEFAULT,
        OPTIONS,
        CONTINUE,
        CONFIRM_NEW_GAME,
        NULL
    }

    public MainMenuState State
    {
        get
        {
            return state;
        }
        set
        {
            previousState = state;
            state = value;
            UpdateState();
        }
    }

    private MainMenuState state;
    private MainMenuState previousState;

    [SerializeField]private GameObject[] mainMenuButtons;
    [SerializeField] private GameObject[] mainVisuals;
    private int _defaultStateIndex;
    public int defaultStateIndex
    {
        get => _defaultStateIndex;
        set
        {
            //Si ce bool n'est pas true, cela signifie que le joueur na même pas commencer l'intro du jeu DONC pas de save
            if (DataPersistenceManager.Instance.gameData.introCinematic == false)
            {
                // IF there are no save data, player can't press CONTINUE button
                _defaultStateIndex = Mathf.Clamp(value, 1, mainMenuButtons.Length - 1);
            }
            else
            {
                if (mainMenuButtons.Length <= 0)
                {
                    _defaultStateIndex = 0;
                    return;
                }

                _defaultStateIndex = Mathf.Clamp(value, 0, mainMenuButtons.Length - 1);
            }
        }
    }

    [SerializeField] private ConfirmNewGameMenu confirmNewGameMenu;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        inputs = InputManager.Instance;
        previousState = MainMenuState.NULL;
        state = MainMenuState.DEFAULT;

        inputs.uiMoveDown.action.performed += IncreaseCurrentIndex;
        inputs.uiMoveUp.action.performed += DecreaseCurrentIndex;
        inputs.uiSubmit.action.performed += Submit;

        inputs.uiMoveUp.action.performed += OnMoveUp;
        inputs.uiMoveDown.action.performed += OnMoveDown;
        inputs.uiMoveLeft.action.performed += OnMoveLeft;
        inputs.uiMoveRight.action.performed += OnMoveRight;
        inputs.uiCancel.action.performed += Cancel;

        if (DataPersistenceManager.Instance.gameData.introCinematic == false)
        {
            defaultStateIndex = 1;
        }
        else
        {
            defaultStateIndex = 0;
        }

            UpdateMainMenuButtons();
    }

    private void OnDisable()
    {
        inputs.uiMoveDown.action.performed -= IncreaseCurrentIndex;
        inputs.uiMoveUp.action.performed -= DecreaseCurrentIndex;
        inputs.uiSubmit.action.performed -= Submit;

        inputs.uiMoveUp.action.performed -= OnMoveUp;
        inputs.uiMoveDown.action.performed -= OnMoveDown;
        inputs.uiMoveLeft.action.performed -= OnMoveLeft;
        inputs.uiMoveRight.action.performed -= OnMoveRight;
        inputs.uiCancel.action.performed -= Cancel;
    }

    public void UpdateState()
    {
        if(previousState != MainMenuState.NULL)
        {
            ExitState(previousState);
        }

        EnterState(state);
    }

    private void EnterState(MainMenuState state)
    {
        switch (state)
        {
            case MainMenuState.OPTIONS:
                options.Open();
                break;

            case MainMenuState.CONFIRM_NEW_GAME:
                confirmNewGameMenu.Open();
                break;

            case MainMenuState.DEFAULT:
                ToggleMainVisuals(true);
                break;
        }
    }

    private void ExitState(MainMenuState state)
    {
        if (state == MainMenuState.CONFIRM_NEW_GAME)
            confirmNewGameMenu.Close();

        if(state == MainMenuState.DEFAULT)
            ToggleMainVisuals(false);
    }

    private void IncreaseCurrentIndex(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(State == MainMenuState.DEFAULT)
        {
            defaultStateIndex += 1;
        }


        UpdateMainMenuButtons();
    }

    private void DecreaseCurrentIndex(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (State == MainMenuState.DEFAULT)
        {
            defaultStateIndex -= 1;
        }

        UpdateMainMenuButtons();
    }

    private void UpdateMainMenuButtons()
    {
        for (int i = 0; i < mainMenuButtons.Length; i++)
        {
            if (i == _defaultStateIndex)
            {
                if(mainMenuButtons[i] !=  null)
                    mainMenuButtons[i].GetComponent<MainMenuButton>().EnableButton();
            }
            else
            {
                if(mainMenuButtons[i] !=  null)
                    mainMenuButtons[i].GetComponent<MainMenuButton>().ResetButton();
            }
        }

        if (DataPersistenceManager.Instance.gameData.introCinematic == false)
        {
            mainMenuButtons[0].GetComponent<MainMenuButtonContinue>().setMatDisabled();
        }
    }

    private void Submit(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(_defaultStateIndex != 0)
        PlaySound(submitSound);
        else if(_defaultStateIndex == 0)
        PlaySound(startGameSound);

        if (State == MainMenuState.DEFAULT)
        {
            if (_defaultStateIndex == 0)
            {
                //Continue
                //Load scene
                StartCoroutine(LoadMainCoroutine());
            }
            else if (_defaultStateIndex == 1)
            {
                //New Game
                StartNewGame();
            }
            else if (_defaultStateIndex == 2)
            {
                //Options
                State = MainMenuState.OPTIONS;
                options.Enable();
            }
            else if (_defaultStateIndex == 3)
            {
                //Quit Game
                Application.Quit();
            }
        }

        else if (State == MainMenuState.OPTIONS)
        {
            options.Submit();
        }

        else if (State == MainMenuState.CONFIRM_NEW_GAME)
        {
            confirmNewGameMenu.Submit();
        }
    }

    private void OnMoveUp(InputAction.CallbackContext ctx)
    {
        PlaySound(navigateSound);

        if (State == MainMenuState.OPTIONS)
            options.MoveUp();

        if (State == MainMenuState.CONFIRM_NEW_GAME)
            confirmNewGameMenu.MoveUp();
    }

    private void OnMoveDown(InputAction.CallbackContext ctx)
    {
        PlaySound(navigateSound);

        if (State == MainMenuState.OPTIONS)
            options.MoveDown();

        if (State == MainMenuState.CONFIRM_NEW_GAME)
            confirmNewGameMenu.MoveDown();
    }

    private void OnMoveLeft(InputAction.CallbackContext ctx)
    {

        PlaySound(mainMenuTweakSound);
        if (State == MainMenuState.OPTIONS)
            options.MoveLeft();
    }

    private void OnMoveRight(InputAction.CallbackContext ctx)
    {
        PlaySound(mainMenuTweakSound);
        if (State == MainMenuState.OPTIONS)
            options.MoveRight();
    }

    private void Cancel(InputAction.CallbackContext ctx)
    {

        if (State == MainMenuState.OPTIONS)
            options.Cancel();


        else if (State == MainMenuState.CONFIRM_NEW_GAME)
            confirmNewGameMenu.Cancel();
    }

    public void OnBack(InputAction.CallbackContext ctx)
    {
        if (State != MainMenuState.OPTIONS)
            return;

        if (options.HandleCancel())
            return;

        // Sinon : vrai back du menu
        options.Close();
        State = MainMenuState.DEFAULT;
    }
    private void StartNewGame()
    {
        Debug.Log("StartNewGame called");
        if (DataPersistenceManager.Instance.gameData.introCinematic == true)
        {
            State = MainMenuState.CONFIRM_NEW_GAME;
            return;
        }


        DataPersistenceManager.Instance.NewGame();
        StartCoroutine(LoadMainCoroutine());
    }

    public IEnumerator LoadMainCoroutine()
    {
        MusicManager.Instance.StopMusic();
        yield return new WaitForSeconds(0.75f);
        SceneManager.LoadScene("Main");
    }

    private void ToggleMainVisuals(bool toggleValue)
    {
        foreach(GameObject visual in mainVisuals)
        {
            visual.SetActive(toggleValue);
        }
    }
    public void PlaySound(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound);
    }
}
