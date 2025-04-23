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

    private void OnEnable()
    {
        thrust.action.Enable();
        turn.action.Enable();
        brake.action.Enable();
        dive.action.Enable();
    }

    private void OnDisable()
    {
        thrust.action.Disable();
        turn.action.Disable();
        brake.action.Disable();
        jump.action.Disable();
    }
}
