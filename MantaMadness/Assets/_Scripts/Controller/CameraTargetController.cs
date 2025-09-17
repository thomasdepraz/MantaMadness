using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTargetController : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Sensitivity for mouse movement.")]
    public float sensitivity = 100f;

    [Tooltip("Minimum pitch angle (looking down).")]
    public float minPitch = -45f;

    [Tooltip("Maximum pitch angle (looking up).")]
    public float maxPitch = 45f;

    [Header("Input Action Asset")]
    [Tooltip("Reference to the InputAction for looking (Vector2).")]
    public InputActionProperty lookAction;

    public Transform target;

    private float pitch;
    private float yaw;

    private void OnEnable()
    {
        if (lookAction != null)
            lookAction.action.Enable();
    }

    private void OnDisable()
    {
        if (lookAction != null)
            lookAction.action.Disable();
    }

    private void Update()
    {
        if (lookAction == null) return;

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        // Apply sensitivity and deltaTime
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        // Adjust yaw and pitch
        yaw += mouseX;
        pitch -= mouseY;

        // Clamp pitch
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Apply rotation
        target.localRotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}