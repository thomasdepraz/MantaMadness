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

    private InputActionMap playerActionsMap;

    private void OnEnable()
    {
        playerActionsMap = InputSystem.actions.FindActionMap("Player");
        if (playerActionsMap != null)
        {
            playerActionsMap.Enable();
        }
    }

    private void OnDisable()
    {
        if (playerActionsMap != null)
        {
            playerActionsMap.Disable();
        }
    }
}
