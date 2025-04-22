using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MantaController : MonoBehaviour
{
    private InputManager m_Inputs;

    private new Rigidbody rigidbody;

    [Header("General")]
    public float mass;
    public float tireFrictionFactor = 0.01f;

    [Header("Suspension")]
    public float suspensionLength;
    public float wheelRadius;
    private float springRestLength;
    public float springStrength;
    public float springDamper;
    public Transform tireObject;
    private float yOffset;

    [Header("Steering")]
    public float steeringSpeed;
    [Range(0f, 1f)] public float angularDrag;
    [Range(0f, 1f)] public float gripFactor;

    [Header("Acceleration")]
    public float accelerationFactor;
    public float maxSpeed;
    public AnimationCurve powerCurve;

    private float accelerationInput, steeringInput = 0;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        m_Inputs = InputManager.Instance;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        accelerationInput = m_Inputs.thrust.action.ReadValue<float>();
        steeringInput = m_Inputs.turn.action.ReadValue<float>();

        if(steeringInput != 0)
            rigidbody.AddTorque(0, steeringInput * steeringSpeed * Mathf.Deg2Rad, 0);
        else
            rigidbody.angularVelocity *= (1 - angularDrag);

        //Vector3 springDir = transform.up;
        Vector3 tireVelocity = rigidbody.GetPointVelocity(transform.position);

        //offset = springRestLength - tireRay.distance;
        //Debug.DrawRay(transform.position, -transform.up * (springRestLength - offset), Color.red, Time.deltaTime);
        //float velocity = Vector3.Dot(springDir, tireVelocity);

        //float force = (offset * springStrength) - (velocity * springDamper);

        ////move tire
        //tireObject.localPosition = new Vector3(tireObject.localPosition.x, yOffset + offset, tireObject.localPosition.z);


        ////suspension
        //carRigidbody.AddForceAtPosition(springDir * force, transform.position);

        Vector3 steeringDir = transform.right;
        float steeringVelocity = Vector3.Dot(steeringDir, tireVelocity);
        float desiredVelocityChange = -steeringVelocity * gripFactor;
        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

        //steering
        rigidbody.AddForceAtPosition(steeringDir * mass * desiredAcceleration, transform.position);

        Vector3 accelerationDir = transform.forward;

        float carSpeed = Vector3.Dot(transform.forward, rigidbody.linearVelocity);
        if (Mathf.Abs(accelerationInput) > 0 && carSpeed < maxSpeed)
        {
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / maxSpeed);
            float availableTorque = powerCurve.Evaluate(normalizedSpeed) * accelerationInput;

            //acceleration/braking
            rigidbody.AddForceAtPosition(accelerationDir * availableTorque * accelerationFactor, transform.position);
        }

        //Simulate friction
        //rigidbody.AddForceAtPosition(-rigidbody.linearVelocity.normalized * tireFrictionFactor, transform.position, ForceMode.Force);
    }



}
