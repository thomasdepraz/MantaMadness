using Unity.VisualScripting;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [Header("Parameters")]
    [SerializeField] private ControllerData controllerData;

    public Vector3 Velocity => this.rb.linearVelocity;
    public Vector3 AngularVelocity => this.rb.angularVelocity;

    private InputManager inputs;
    private float defaultDrag;

    private void Start()
    {
        inputs = InputManager.Instance;
        defaultDrag = rb.linearDamping;
    }

    private void FixedUpdate()
    {
        //Get inputs
        float thrust = inputs.thrust.action.ReadValue<float>();
        float turn = inputs.turn.action.ReadValue<float>();
        float brake = inputs.brake.action.ReadValue<float>();

        float inputMagnitude = Mathf.Max(Mathf.Abs(thrust), Mathf.Abs(turn));
        float speed = controllerData.baseSpeed * controllerData.baseSpeedModifier;

        float forward = 0.0f;
        if (thrust > 0.0)
        {
            forward = thrust * speed;
        }

        float steer = turn * controllerData.baseTurnSpeed;

        //Apply forces
        rb.AddForce(transform.forward * forward * 0.02f);
        rb.AddTorque(new Vector3(0.0f, steer * 0.02f, 0.0f));

        //Apply drag if braking
        if (brake > 0.0f)
        {
            rb.linearDamping = Mathf.Lerp(defaultDrag, controllerData.brakeForce, brake);
        }
        else
        {
            rb.linearDamping = defaultDrag;
        }
    }
}
