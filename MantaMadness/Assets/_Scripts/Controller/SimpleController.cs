using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [Header("Parameters")]
    public float baseMovementSpeed;
    public float baseMovementModifier;
    public float baseReverseSpeedModifer;
    public float baseTurnSpeed;

    public Vector3 Velocity => this.rb.linearVelocity;
    public Vector3 AngularVelocity => this.rb.angularVelocity;

    InputAction movement;

    private void Start()
    {

    }

    private void FixedUpdate()
    {
        float thrust = InputManager.Instance.thrust.action.ReadValue<float>();
        float turn = InputManager.Instance.turn.action.ReadValue<float>();

        float inputMagnitude = Mathf.Max(Mathf.Abs(thrust), Mathf.Abs(turn));
        float speed = baseMovementSpeed * baseMovementModifier;

        float forward = 0.0f;
        if (thrust > 0.0)
        {
            forward = thrust * speed;
        }
        else if(thrust < 0.0f)
        {
            forward = thrust * baseReverseSpeedModifer;
        }

        float steer = turn * baseTurnSpeed;
        float scaledSpeed = speed * 0.0001f;

        rb.AddForce(transform.forward * forward * 0.02f);
        rb.AddTorque(new Vector3(0.0f, steer * 0.02f, 0.0f));
    }
}
