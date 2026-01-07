using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
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
    }
    [Header("Controller Actions")]
    public InputActionReference moveDirection;
    public InputActionReference thrust;
    public InputActionReference turn;
    public InputActionReference brake;
    public InputActionReference boost;
    public InputActionReference jump;
    public InputActionReference airControl;
    public InputActionReference dash;
    public InputActionReference drift;
    public InputActionReference strafLeft;
    public InputActionReference strafRight;
    public InputActionReference resetCamera;
    public InputActionReference stomp;
    public InputActionReference interact;
    public InputActionReference driftR;
    public InputActionReference driftL;

    private InputActionMap playerActionsMap;

    [Header("UI / Menu Actions")]
    private InputActionMap uiActionsMap;

    public InputActionReference uiMoveUp;
    public InputActionReference uiMoveDown;
    public InputActionReference uiMoveLeft;
    public InputActionReference uiMoveRight;
    public InputActionReference uiSubmit;
    public InputActionReference uiCancel;

    private void OnEnable()
    {
        playerActionsMap = InputSystem.actions.FindActionMap("Player");
        if (playerActionsMap != null)
        {
            playerActionsMap.Enable();
        }

        uiActionsMap = InputSystem.actions.FindActionMap("UI");
        if (uiActionsMap != null)
        {
            uiActionsMap.Enable();
        }
    }

    private void OnDisable()
    {
        if (playerActionsMap != null)
        {
            playerActionsMap.Disable();
        }

        if (uiActionsMap != null)
        {
            uiActionsMap.Disable();
        }
    }
}
