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
        //inputs.resetCamera.action.performed += ResetCamPos;
    }

    private void OnEnable()
    {
        if (lookAction != null)
            lookAction.action.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        if(playerActionsMap != null)
        playerActionsMap.actionTriggered += OnActionPerformed;

        //if(inputs != null)
        //inputs.resetCamera.action.performed += ResetCamPos;
    }

    private void OnDisable()
    {
        if (lookAction != null)
            lookAction.action.Disable();

        playerActionsMap.actionTriggered -= OnActionPerformed;
        //inputs.resetCamera.action.performed -= ResetCamPos;
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

        if(Game.Instance.player.State == ControllerState.STOMP)
        {
            minPitch = _data.stomp_minPitch;
            maxPitch = _data.stomp_maxPitch;
        }
        else if (Game.Instance.player.State == ControllerState.FALLING)
        {
            minPitch = _data.fall_minPitch;
            maxPitch = _data.fall_maxPitch;
        }
        else
        {
            minPitch = _data.minPitch;
            maxPitch = _data.maxPitch;
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

        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, smoothValue);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, smoothValue);
        // Apply rotation
        if(ResetCamRoutine == null)
        {
            Vector3 targetUp = player.hoverBehaviour.normalContainer.up;
            currentUp = Vector3.Slerp(currentUp, targetUp, Time.deltaTime * 5f);
        }
        //else
        //{
        //    Vector3 targetUp = player.transform.rotation.eulerAngles;
        //    currentUp = Vector3.Slerp(currentUp, targetUp, Time.deltaTime * 50f);
        //}

    }

    private void FixedUpdate()
    {
        target.position = player.transform.position + offset;
        target.up = currentUp;

        if (toggleFixedCam == false)
        {
            //Quaternion rotation = Quaternion.Euler(currentUp.x + pitch, currentUp.y + yaw, currentUp.z);
            //target.rotation = rotation;
            target.Rotate(new Vector3(currentUp.x + pitch, currentUp.y + yaw, currentUp.z));
        }
        else
        {
            //Quaternion rotation = Quaternion.Euler(player.transform.rotation.eulerAngles);
            //target.rotation = rotation;
            target.Rotate(player.transform.rotation.eulerAngles);
            // ---- On ajoute seulement cette ligne ----
            StretchyCamBehavior(lookAction.action.ReadValue<Vector2>());
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
}