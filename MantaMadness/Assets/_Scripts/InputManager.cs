using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static DialogLoader;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public static InputDeviceType CurrentDevice { get; private set; }

    public static event Action<InputDeviceType> OnDeviceChanged;

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

        interact.action.performed += UpdateCurrentDevice;
        jump.action.performed += UpdateCurrentDevice;
        dash.action.performed += UpdateCurrentDevice;
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

        interact.action.performed -= UpdateCurrentDevice;
        jump.action.performed -= UpdateCurrentDevice;
        dash.action.performed -= UpdateCurrentDevice;
    }
    private void SetDevice(InputDeviceType newDevice)
    {
        if (CurrentDevice == newDevice) return;

        CurrentDevice = newDevice;
        OnDeviceChanged?.Invoke(CurrentDevice);
    }

    public void UpdateCurrentDevice(InputAction.CallbackContext context)
    {
        var device = context.control?.device;
        if (device == null) return;

        if (device is Keyboard || device is Mouse)
        {
            SetDevice(InputDeviceType.KeyboardMouse);
            return;
        }

        if (device is Gamepad)
        {
            string product = device.description.product?.ToLowerInvariant() ?? "";
            string manufacturer = device.description.manufacturer?.ToLowerInvariant() ?? "";

            if (product.Contains("dualshock") || product.Contains("dualsense") ||
                manufacturer.Contains("sony") || product.Contains("playstation"))
            {
                SetDevice(InputDeviceType.PlayStation);
                return;
            }

            SetDevice(InputDeviceType.Xbox);
        }
    }
}
