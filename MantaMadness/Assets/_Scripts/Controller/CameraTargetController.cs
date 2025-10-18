using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTargetController : MonoBehaviour
{
    [SerializeField]
    private CameraControllerData _data;

    [Header("Rotation Settings")]
    [Tooltip("Sensitivity for mouse movement.")]
    public float sensitivity = 100f;

    [Tooltip("Minimum pitch angle (looking down).")]
    public float minPitch = -45f;

    [Tooltip("Maximum pitch angle (looking up).")]
    public float maxPitch = 45f;

    [Tooltip("Smooth Value for movement.")]
    public float smoothValue = 0.08f;

    [Header("Input Action Asset")]
    [Tooltip("Reference to the InputAction for looking (Vector2).")]
    public InputActionProperty lookAction;

    public Transform target;

    private float pitch;
    private float yaw;

    static float yawVelocity = 0f;
    static float pitchVelocity = 0f;

    private SimpleController player;

    private bool isControllerDevice = false;
    private InputActionMap playerActionsMap;

    private Vector3 currentUp;
    [SerializeField] private Vector3 offset;


    private void OnEnable()
    {
        if (lookAction != null)
            lookAction.action.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        playerActionsMap = InputSystem.actions.FindActionMap("Player");
        playerActionsMap.actionTriggered += OnActionPerformed;
    }

    private void Start()
    {
        if (player == null)
        {
            player = Game.Instance.player;
        }

        currentUp = target.transform.up;
    }

    private void OnDisable()
    {
        if (lookAction != null)
            lookAction.action.Disable();
    }
    private void OnActionPerformed(InputAction.CallbackContext context)
    {
        InputDevice device = context.control.device;

        if (device is Keyboard)
        {
            isControllerDevice = false;
        }
        else if (device is Gamepad gamepad)
        {
            isControllerDevice = true;
        }
    }

    private void Update()
    {
        if (lookAction == null) return;

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        if(isControllerDevice)
        {
            sensitivity = _data.sensitivity_controller;
        }
        else
        {
            sensitivity = _data.sensitivity;
        }

        minPitch = _data.minPitch;
        maxPitch = _data.maxPitch;
        smoothValue = _data.smooth;


        // Apply sensitivity and deltaTime
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        float targetYaw = yaw + mouseX;
        float targetPitch = pitch - mouseY;

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        yaw = Mathf.SmoothDamp(yaw, targetYaw,ref yawVelocity,  smoothValue);
        pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, smoothValue);
        // Apply rotation
        Vector3 targetUp = player.hoverBehaviour.normalContainer.up;
        currentUp = Vector3.Slerp(currentUp, targetUp, Time.deltaTime * 5f);  
    }

    private void FixedUpdate()
    {
        target.up = currentUp;
        target.position = player.transform.position + offset;
        target.Rotate(new Vector3(currentUp.x + pitch, currentUp.y + yaw, currentUp.z));
    }
}