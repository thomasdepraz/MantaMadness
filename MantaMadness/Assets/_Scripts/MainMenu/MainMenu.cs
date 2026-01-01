using System.Linq;
using Unity.Mathematics.Geometry;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;
using static UnityEngine.CullingGroup;

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
    private int _defaultStateIndex;
    public int defaultStateIndex
    {
        get => _defaultStateIndex;
        set
        {
            if(mainMenuButtons.Length <= 0)
            {
                _defaultStateIndex = 0;
                return;
            }

            _defaultStateIndex = Mathf.Clamp(value, 0, mainMenuButtons.Length - 1);
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
        EnterState(state);

        if(previousState != MainMenuState.NULL)
        {
            ExitState(previousState);
        }
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
        }
    }

    private void ExitState(MainMenuState state)
    {
        if (state == MainMenuState.CONFIRM_NEW_GAME)
            confirmNewGameMenu.Close();
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
    }

    private void Submit(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(State == MainMenuState.DEFAULT)
        {
            if(_defaultStateIndex == 0)
            {
                //Continue
                //Load scene
                SceneManager.LoadScene("Main");
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

        else if(State == MainMenuState.OPTIONS)
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
        if (State == MainMenuState.OPTIONS)
            options.MoveUp();

        if (State == MainMenuState.CONFIRM_NEW_GAME)
            confirmNewGameMenu.MoveUp();
    }

    private void OnMoveDown(InputAction.CallbackContext ctx)
    {
        if (State == MainMenuState.OPTIONS)
            options.MoveDown();

        if (State == MainMenuState.CONFIRM_NEW_GAME)
            confirmNewGameMenu.MoveDown();
    }

    private void OnMoveLeft(InputAction.CallbackContext ctx)
    {
        if (State == MainMenuState.OPTIONS)
            options.MoveLeft();
    }

    private void OnMoveRight(InputAction.CallbackContext ctx)
    {
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
        if (DataPersistenceManager.Instance.HasGameData())
        {
            State = MainMenuState.CONFIRM_NEW_GAME;
            return;
        }


        DataPersistenceManager.Instance.NewGame();
        SceneManager.LoadScene("Main");
    }

}
