using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTargetController : MonoBehaviour
{

    public static CameraTargetController instance;
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

    private float minYaw;
    private float maxYaw;

    static float yawVelocity = 0f;
    static float pitchVelocity = 0f;

    private SimpleController player;

    private bool isControllerDevice = false;
    private InputActionMap playerActionsMap;

    private Vector3 currentUp;
    private Vector3 currentForward;
    [SerializeField] private Vector3 offset;

    [SerializeField]private float stretchyYaw = 0f;
    [SerializeField] private float stretchyPitch = 0f;
    [SerializeField] private float stretchyReturnSpeed = 12f;    // vitesse de retour vers neutre
    [SerializeField] private float stretchStrengthHorizontal = 35f;
    [SerializeField] private float stretchStrengthUp = 25f;
    [SerializeField] private float stretchStrengthDown = 45f;
    [SerializeField] private float stretchyDamp = 0.1f;

    [Header("Mouse Parameters")]
    [SerializeField] private float stretchMouseStrengthHorizontal = 2.5f;
    [SerializeField] private float stretchMouseStrengthUp = 2.0f;
    [SerializeField] private float stretchMouseStrengthDown = 3.0f;

    private bool toggleFixedCam;

    private float railYawOffset = 0f;

    private InputManager inputs;

    public GameObject mantavisual;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }


    }
    private void Start()
    {
        currentUp = target.transform.up;
        if (inputs == null)
            inputs = InputManager.Instance;

        if (player == null)
            player = Game.Instance.player;

        playerActionsMap = InputSystem.actions.FindActionMap("Player");
        playerActionsMap.actionTriggered += OnActionPerformed;
    }

    private void OnEnable()
    {
        if (lookAction != null)
            lookAction.action.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        if(playerActionsMap != null)
        playerActionsMap.actionTriggered += OnActionPerformed;
    }

    private void OnDisable()
    {
        if (lookAction != null)
            lookAction.action.Disable();

        playerActionsMap.actionTriggered -= OnActionPerformed;
        StopAllCoroutines();
        ResetCamRoutine = null;
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

    private ControllerState currentState = ControllerState.FALLING;
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

        smoothValue = _data.smooth;
        ControllerState state = Game.Instance.player.State;
        if(currentState != state)
        {
            currentState = state;
            //if (state == ControllerState.RAIL)
            //    yaw = WrapAngle(player.hoverBehaviour.normalContainer.eulerAngles.y);
        }


        if (state == ControllerState.STOMP)
        {
            minPitch = _data.stomp_minPitch;
            maxPitch = _data.stomp_maxPitch;
            minYaw = 0;
            maxYaw = 0;
        }
        else if (state == ControllerState.FALLING)
        {
            minPitch = _data.fall_minPitch;
            maxPitch = _data.fall_maxPitch;
            minYaw = 0;
            maxYaw = 0;
        }
        else if(state == ControllerState.RAIL)
        {
            minPitch = _data.minPitch;
            maxPitch = _data.maxPitch;

            var y = player.hoverBehaviour.normalContainer.eulerAngles.y;
            minYaw = y + _data.rail_minYew;
            maxYaw = y + _data.rail_maxYew;   
        }
        else
        {
            minPitch = _data.minPitch;
            maxPitch = _data.maxPitch;
            minYaw = 0;
            maxYaw = 0;
        }
        // Apply sensitivity and deltaTime
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        float targetYaw = yaw + mouseX;
        float targetPitch = 0f;

        //Invert or not
        if(PlayerPrefs.GetInt("invertAxis",0) == 0)
        {
            targetPitch = pitch - mouseY;
        }
        else
        {
            targetPitch = pitch + mouseY;
        }

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        if(state == ControllerState.RAIL)
        {
            var centerYaw = WrapAngle(player.hoverBehaviour.normalContainer.eulerAngles.y);
            var deltaYaw = Mathf.DeltaAngle(centerYaw, targetYaw);
            deltaYaw = Mathf.Clamp(deltaYaw, _data.rail_minYew, _data.rail_maxYew);
            targetYaw = WrapAngle(centerYaw + deltaYaw);

            //yaw = targetYaw;
        }

        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, smoothValue);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, smoothValue);
        // Apply rotation
        if(ResetCamRoutine == null)
        {
            Vector3 targetUp = player.hoverBehaviour.normalContainer.up;
            currentUp = Vector3.Slerp(currentUp, targetUp, Time.deltaTime * 5f);
        }
    }

    private float WrapAngle(float angle)
    {
        if (angle > 180) return angle - 360;
        else if (angle < -180) return angle + 360;

        return angle;
    }

    public void SyncYawPitchToPlayerFacing()
    {
        if (player == null)
            return;

        yaw = WrapAngle(player.transform.eulerAngles.y);
    }

    private void ApplyTargetRotation()
    {
        Vector3 referenceForward = Vector3.ProjectOnPlane(Vector3.forward, currentUp);
        if (referenceForward.sqrMagnitude < 0.0001f)
        {
            referenceForward = Vector3.ProjectOnPlane(player.hoverBehaviour.normalContainer.forward, currentUp);
        }

        if (referenceForward.sqrMagnitude < 0.0001f)
            referenceForward = Vector3.Cross(currentUp, Vector3.right);

        referenceForward.Normalize();

        Vector3 forward = Quaternion.AngleAxis(yaw, currentUp) * referenceForward;
        Vector3 right = Vector3.Cross(currentUp, forward).normalized;
        Quaternion pitchRotation = Quaternion.AngleAxis(pitch, right);
        target.rotation = pitchRotation * Quaternion.LookRotation(forward, currentUp);
    }

    private void FixedUpdate()
    {
        target.position = player.transform.position + offset;

        bool canRotateCamera = Time.timeScale > 0f;
        if (PauseMenu.instance != null && PauseMenu.instance.isPaused)
            canRotateCamera = false;

        if (canRotateCamera)
        {
            if (toggleFixedCam == false)
            {
                ApplyTargetRotation();
            }
            else
            {
                target.rotation = Quaternion.Euler(0f, player.transform.rotation.eulerAngles.y, 0f);
                StretchyCamBehavior(lookAction.action.ReadValue<Vector2>());
            }
        }
    }

    public void ResetCamPos(bool toggleValue)
    {
        if(toggleFixedCam != toggleValue)
        {
            if (ResetCamRoutine == null)
            {
                ResetCamRoutine = StartCoroutine(ResetCamCoroutine(toggleValue));
            }
        }
    }

    private Coroutine ResetCamRoutine;
    private IEnumerator ResetCamCoroutine(bool toggleValue)
    {
        UIManager.Instance.gameInterface.ToggleBlackBarEffect(toggleValue, 0.5f);
        yield return new WaitForSeconds(0.15f);
        toggleFixedCam = toggleValue;
        // Phase 2 : on garde la rotation actuelle comme nouvelle base
        Vector3 euler = target.rotation.eulerAngles;

        // Comme Unity stocke les angles de 0 à 360°, on les recentre autour de -180 à 180
        float newYaw = euler.y;
        float newPitch = euler.x;
        if (newPitch > 180) newPitch -= 360;
        if (newYaw > 180) newYaw -= 360;

        yaw = newYaw;
        pitch = Mathf.Clamp(newPitch, minPitch, maxPitch);

        // Maintenant, on peut remettre le comportement normal
        ResetCamRoutine = null;

    }

    private void StretchyCamBehavior(Vector2 lookInput)
    {
        Vector2 targetOffset;

        // Si c'est la souris → les deltas sont super sensibles donc on adapte
        if (!isControllerDevice)
        {
            target.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles);
        }
        else
        {
            if (lookInput.sqrMagnitude < 0.01f)
            {
                targetOffset = Vector2.zero;
            }
            else
            {
                float xOffset = lookInput.x * stretchStrengthHorizontal;

                float yStrength = lookInput.y > 0 ? stretchStrengthUp : stretchStrengthDown;
                float yOffset = lookInput.y * yStrength;

                targetOffset = new Vector2(xOffset, yOffset);

                stretchyYaw = Mathf.Lerp(stretchyYaw, targetOffset.x,
                    1f - Mathf.Exp(-stretchyReturnSpeed * Time.deltaTime));
                stretchyPitch = Mathf.Lerp(stretchyPitch, -targetOffset.y,
                    1f - Mathf.Exp(-stretchyReturnSpeed * Time.deltaTime));

                // Applique la rotation offset
                target.Rotate(new Vector3(stretchyPitch, stretchyYaw, 0f));
            }
        }
    }

    public void ToggleDebugMod()
    {
        Cursor.lockState = CursorLockMode.None;
    }
}