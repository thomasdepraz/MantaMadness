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

    public InputActionReference thrust;
    public InputActionReference turn;
    public InputActionReference brake;
    public InputActionReference dive;
    public InputActionReference jump;
    public InputActionReference airControl;
    public InputActionReference drift;

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
