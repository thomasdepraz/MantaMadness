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

    [Tooltip("Minimum pitch angle (looking down).")]
    public float minYawn = -45f;

    [Tooltip("Maximum pitch angle (looking up).")]
    public float maxYawn = 45f;

    [Tooltip("Smooth Value for movement.")]
    public float smoothValue = 10f;

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

        if (player.State == ControllerState.SURFING)
        {
            if(player.HorizontalVelocity.magnitude > 5f)
            {
                if(isControllerDevice)
                {
                    sensitivity = _data.surf_sensitivity_controller;
                }
                else
                {
                    sensitivity = _data.surf_sensitivity;
                }

                minPitch = _data.surf_minPitch;
                maxPitch = _data.surf_maxPitch;
                minYawn = _data.surf_minYaw;
                maxYawn = _data.surf_maxYaw;
                smoothValue = _data.surf_smooth;


                // Apply sensitivity and deltaTime
                float mouseX = lookInput.x * sensitivity * Time.deltaTime;
                float mouseY = lookInput.y * sensitivity * Time.deltaTime;

                float targetYaw = yaw + mouseX;
                float targetPitch = pitch - mouseY;

                targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
                targetYaw = Mathf.Clamp(targetYaw, minYawn, maxYawn);


                yaw = Mathf.SmoothDamp(yaw, targetYaw,ref yawVelocity,  smoothValue);
                pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, smoothValue);
            }
            else
            {
                if (isControllerDevice)
                {
                    sensitivity = _data.idle_sensitivity_controller;
                }
                else
                {
                    sensitivity = _data.idle_sensitivity;
                }
                minPitch = _data.idle_minPitch;
                maxPitch = _data.idle_maxPitch;
                minYawn = _data.idle_minYaw;
                maxYawn = _data.idle_maxYaw;
                smoothValue = _data.idle_smooth;

                // Apply sensitivity and deltaTime
                float mouseX = lookInput.x * sensitivity * Time.deltaTime;
                float mouseY = lookInput.y * sensitivity * Time.deltaTime;

                float targetYaw = yaw + mouseX;
                float targetPitch = pitch - mouseY;

                targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

                yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawVelocity, smoothValue);
                pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, smoothValue);
            }

        }

        else if (player.State == ControllerState.SWIMMING)
        {
            if (isControllerDevice)
            {
                sensitivity = _data.swim_sensitivity_controller;
            }
            else
            {
                sensitivity = _data.swim_sensitivity;
            }
            minPitch = _data.swim_minPitch;
            maxPitch = _data.swim_maxPitch;
            minYawn = _data.swim_minYaw;
            maxYawn = _data.swim_maxYaw;
            smoothValue = _data.swim_smooth;


            // Apply sensitivity and deltaTime
            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;

            float targetYaw = yaw + mouseX;
            float targetPitch = pitch - mouseY;

            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            targetYaw = Mathf.Clamp(targetYaw, minYawn, maxYawn);

            yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawVelocity, smoothValue);
            pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, smoothValue);
        }


        else if (player.State == ControllerState.JUMPING)
        {
            if (isControllerDevice)
            {
                sensitivity = _data.jump_sensitivity_controller;
            }
            else
            {
                sensitivity = _data.jump_sensitivity;
            }
            minPitch = _data.jump_minPitch;
            maxPitch = _data.jump_maxPitch;
            minYawn = _data.jump_minYaw;
            maxYawn = _data.jump_maxYaw;
            smoothValue = _data.jump_smooth;


            // Apply sensitivity and deltaTime
            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;

            float targetYaw = yaw + mouseX;
            float targetPitch = pitch - mouseY;

            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
            targetYaw = Mathf.Clamp(targetYaw, minYawn, maxYawn);

            yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawVelocity, smoothValue);
            pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, smoothValue);
        }

        else if(player.State == ControllerState.FALLING)
        {
            if (isControllerDevice)
            {
                sensitivity = _data.fall_sensitivity_controller;
            }
            else
            {
                sensitivity = _data.fall_sensitivity;
            }
            minPitch = _data.fall_minPitch;
            maxPitch = _data.fall_maxPitch;
            minYawn = _data.fall_minYaw;
            maxYawn = _data.fall_maxYaw;
            smoothValue = _data.fall_smooth;


            // Apply sensitivity and deltaTime
            float mouseX = lookInput.x * sensitivity * Time.deltaTime;
            float mouseY = lookInput.y * sensitivity * Time.deltaTime;

            float targetYaw = yaw + mouseX;
            float targetPitch = pitch - mouseY;

            targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

            yaw = Mathf.SmoothDamp(yaw, targetYaw, ref yawVelocity, smoothValue);
            pitch = Mathf.SmoothDamp(pitch, targetPitch, ref pitchVelocity, smoothValue);
        }
            // Apply rotation
            target.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}