using UnityEngine;

public enum ControllerState
{
    FALLING, 
    JUMPING,
    SURFING,
    DIVING, 
    SWIMMING,
}

public class SimpleController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [Header("Parameters")]
    [SerializeField] private ControllerData controllerData;

    public Vector3 Velocity => this.rb.linearVelocity;
    public Vector3 AngularVelocity => this.rb.angularVelocity;

    public ControllerState State {
        get
        { 
            return state; 
        }
        set
        {
            Debug.Log($"Previous state = {state} // new state : {value}");
            state = value;
        }
    }

    private InputManager inputs;
    private float defaultDrag;
    float thrust, turn, brake = 0f;

    private ControllerState state;
    private WaterBlock currentWaterBlock;
    private int jumpCount;

    private void Start()
    {
        inputs = InputManager.Instance;
        defaultDrag = rb.linearDamping;
        State = ControllerState.FALLING;

        inputs.dive.action.performed += Dive;
        inputs.dive.action.canceled += DiveReleased;
        inputs.jump.action.performed += Jump;
    }


    private void Jump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (state == ControllerState.DIVING || state == ControllerState.SWIMMING)
            return;

        if(state == ControllerState.FALLING && jumpCount < 1)
        {
            // spin when falling
            state = ControllerState.JUMPING;
            jumpCount++;
            rb.linearVelocity = GetHorizontalVelocity(rb.linearVelocity);
            rb.AddForce(Vector3.forward * controllerData.forwardImpulseForce + Vector3.up * controllerData.upwardImpulseForce, ForceMode.Impulse);
        }

        if(state == ControllerState.SURFING && jumpCount < 1)
        {
            // spin when surfing
            state = ControllerState.JUMPING;
            jumpCount++;
            rb.linearVelocity = GetHorizontalVelocity(rb.linearVelocity);
            rb.AddForce(Vector3.forward * controllerData.forwardImpulseForce + Vector3.up * controllerData.upwardImpulseForce, ForceMode.Impulse);
        }
    }

    private void Dive(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(State == ControllerState.SURFING)
        {
            State = ControllerState.DIVING;
            rb.AddForce(Vector3.down * controllerData.baseDivingForce, ForceMode.Impulse);
        }
    }

    private void DiveReleased(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(State == ControllerState.DIVING && currentWaterBlock != null)
        {
            float currentDepth = currentWaterBlock.GetDepthAtPosition(transform.position, out _);

            //Kill vertical velocity before jumping
            rb.linearVelocity = GetHorizontalVelocity(rb.linearVelocity);

            //Jump impulse
            rb.AddForce(Vector3.up * currentDepth * controllerData.jumpMultiplier, ForceMode.Impulse);

            State = ControllerState.JUMPING;
        }
    }

    private void Update()
    {
        thrust = inputs.thrust.action.ReadValue<float>();
        turn = inputs.turn.action.ReadValue<float>();
        brake = inputs.brake.action.ReadValue<float>();
    }

    bool hasHit = false;
    private void FixedUpdate()
    {
        hasHit = Physics.Raycast(transform.position, -transform.up, out RaycastHit info, controllerData.hoverRaycastLength);

        if (State == ControllerState.FALLING)
        {
            if (hasHit)
            {
                State = ControllerState.SURFING;
                jumpCount = 0;
            }
        }

        if(State == ControllerState.JUMPING && rb.linearVelocity.y < 0 && currentWaterBlock == null)
        {
            State = ControllerState.FALLING;
        }

        //Apply gravity
        if (State == ControllerState.JUMPING ||
            State ==  ControllerState.FALLING ||
            State == ControllerState.SURFING)
        {
            rb.AddForce(Vector3.down * 9.8f);
        }

        //Hover on water
        if (State == ControllerState.SURFING)
        {
            if(hasHit)
            {
                SurfHover(info);
            }
            else
            {
                State = ControllerState.FALLING;
            }
        }

        //diving y force 
        if(State == ControllerState.DIVING && currentWaterBlock != null)
        {
            float currentDepth = currentWaterBlock.GetDepthAtPosition(transform.position, out _);
            if (currentDepth > 0 && rb.linearVelocity.y < 0)
            {
                if (currentDepth < controllerData.baseDivingDepth)
                {
                    //Drag on y velocity - the deeper the higher the drag 
                    rb.AddForce(new Vector3(0.0f, -rb.linearVelocity.y, 0.0f) * (currentDepth / controllerData.baseDivingDepth) * controllerData.underwaterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                }
                else if (currentDepth > controllerData.baseDivingDepth)
                {
                    //Stop when hitting max depth
                    rb.AddForce(new Vector3(0.0f, -rb.linearVelocity.y, 0.0f), ForceMode.VelocityChange);
                }
            }
        }

        //movement
        if(State == ControllerState.SURFING || State == ControllerState.DIVING)
        {
            float speed = controllerData.baseSpeed * controllerData.baseSpeedModifier;

            float forward = 0.0f;
            if (thrust > 0.0)
            {
                forward = thrust * speed;
            }

            float steer = turn * controllerData.baseTurnSpeed;

            float steeringVelocity = Vector3.Dot(transform.right, Velocity);
            float desiredVelocityChange = -steeringVelocity * controllerData.gripForce * Time.fixedDeltaTime;

            //Apply forces (grip - thrust - steer)
            rb.AddForce(transform.right * desiredVelocityChange, ForceMode.VelocityChange);
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

    private void SurfHover(RaycastHit info)
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 rayDir = -transform.up;

        Vector3 otherVelocity = Vector3.zero;
        Rigidbody otherRb = info.rigidbody;
        if (otherRb != null)
        {
            otherVelocity = otherRb.linearVelocity;
        }

        float rayDirVel = Vector3.Dot(rayDir, velocity);
        float otherDirVel = Vector3.Dot(rayDir, otherVelocity);

        float relativeVelocity = rayDirVel - otherDirVel;
        float x = info.distance - controllerData.hoverHeight;
        float springForce = (x * controllerData.hoverStrength) - (relativeVelocity * controllerData.hoverDamper);
        rb.AddForce(rayDir * springForce);

        //add force on touched object
        if (otherRb != null)
        {
            otherRb.AddForceAtPosition(rayDir * -springForce, info.point);
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (false == collision.gameObject.TryGetComponent<WaterBlock>(out WaterBlock waterBlock))
        {
            return;
        }

        currentWaterBlock = waterBlock;

        if(State == ControllerState.FALLING)
        {
            float currentDepth = currentWaterBlock.GetDepthAtPosition(transform.position, out _);

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * currentDepth * controllerData.jumpMultiplier, ForceMode.Impulse);

            State = ControllerState.JUMPING;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<WaterBlock>(out _))
            currentWaterBlock = null;

        if (State != ControllerState.JUMPING && hasHit == false)
        {
            State = ControllerState.FALLING;
        }
    }

    private Vector3 GetHorizontalVelocity(Vector3 vel)
    {
        return new Vector3(vel.x, 0, vel.z);
    }
}
