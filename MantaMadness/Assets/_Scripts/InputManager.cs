using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem.XInput;
using UnityEngine.SceneManagement;
using static DialogLoader;
using static Unity.Burst.Intrinsics.X86.Avx;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public static InputDeviceType CurrentDevice { get; private set; }

    public static event Action<InputDeviceType> OnDeviceChanged;

    [SerializeField] private InputActionAsset inputActions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
    public InputActionReference pause;

    private InputActionMap playerActionsMap;

    [Header("UI / Menu Actions")]
    private InputActionMap uiActionsMap;

    public InputActionReference uiMoveUp;
    public InputActionReference uiMoveDown;
    public InputActionReference uiMoveLeft;
    public InputActionReference uiMoveRight;
    public InputActionReference uiSubmit;
    public InputActionReference uiCancel;
    public InputActionReference uiPause;

    private void OnEnable()
    {
        InputSystem.onAnyButtonPress.CallOnce(ctrl =>
        {
            UpdateDeviceFromControl(ctrl);
        });

        playerActionsMap = inputActions.FindActionMap("Player", true);
        if (playerActionsMap != null)
        {
            //playerActionsMap.Enable();
        }

        uiActionsMap = inputActions.FindActionMap("UI", true);
        if (uiActionsMap != null)
        {
            //uiActionsMap.Enable();
        }

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            EnableGameplay();
            interact.action.Enable();
            jump.action.Enable();
            dash.action.Enable();
            moveDirection.action.Enable();
            interact.action.performed += UpdateCurrentDevice;
            jump.action.performed += UpdateCurrentDevice;
            dash.action.performed += UpdateCurrentDevice;
            moveDirection.action.performed += UpdateCurrentDevice;
        }
        else if(SceneManager.GetActiveScene().name == "MainMenu")
        {
            EnableUI();
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

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            interact.action.performed -= UpdateCurrentDevice;
            jump.action.performed -= UpdateCurrentDevice;
            dash.action.performed -= UpdateCurrentDevice;
            moveDirection.action.performed -= UpdateCurrentDevice;
        }
    }

    private void UpdateDeviceFromControl(InputControl control)
    {
        var device = control.device;
        if (device == null) return;

        if (device is Keyboard || device is Mouse)
            SetDevice(InputDeviceType.KeyboardMouse);
        else if (device is XInputController)
            SetDevice(InputDeviceType.Xbox);
        else if (device is DualShockGamepad || device is DualSenseGamepadHID)
            SetDevice(InputDeviceType.PlayStation);
        else if (device is Gamepad)
            SetDevice(InputDeviceType.Xbox);
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

        // Keyboard / Mouse
        if (device is Keyboard || device is Mouse)
        {
            SetDevice(InputDeviceType.KeyboardMouse);
            return;
        }

        // Xbox (XInput)
        if (device is XInputController)
        {
            SetDevice(InputDeviceType.Xbox);
            return;
        }

        // PlayStation (native)
        if (device is DualShockGamepad || device is DualSenseGamepadHID)
        {
            SetDevice(InputDeviceType.PlayStation);
            return;
        }

        // Generic Gamepad (Steam Input, 8BitDo, autres)
        if (device is Gamepad)
        {
            // Steam Input = Xbox virtuel → on assume Xbox
            SetDevice(InputDeviceType.Xbox);
            return;
        }
    }

    public void EnableGameplay()
    {
        if (playerActionsMap.enabled) return;

        playerActionsMap.Enable();
        uiActionsMap.Disable();
    }

    public void EnableUI()
    {
        if (uiActionsMap.enabled) return;

        playerActionsMap.Disable();
        uiActionsMap.Enable();
    }
}
