using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

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
    public Vector3 HorizontalVelocity => new Vector3(this.rb.linearVelocity.x, 0f, this.rb.linearVelocity.z);
    public Vector3 AngularVelocity => this.rb.angularVelocity;
    public float CurrentDepth => currentWaterBlock is null ? 0 : currentWaterBlock.GetDepthAtPosition(transform.position, out _);
    public float MaxDepth => currentWaterBlock is null ? 0 : maxDivingDepth;

    public ControllerState State {
        get
        { 
            return state; 
        }
        set
        {
            stateChanged.Invoke(state, value);
            state = value;
        }
    }

    private InputManager inputs;
    private float defaultDrag;
    float thrust, turn, brake = 0f;
    Vector2 airControl;

    private ControllerState state;
    private WaterBlock currentWaterBlock;
    private float maxDivingDepth;
    private float maxDepth;
    private int jumpCount;

    public Action<ControllerState, ControllerState> stateChanged;

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
            rb.linearVelocity = HorizontalVelocity;
            rb.AddForce(Vector3.forward * controllerData.forwardImpulseForce + Vector3.up * controllerData.upwardImpulseForce, ForceMode.Impulse);
        }

        if(state == ControllerState.SURFING && jumpCount < 1)
        {
            // spin when surfing
            state = ControllerState.JUMPING;
            jumpCount++;
            rb.linearVelocity = HorizontalVelocity;
            rb.AddForce(Vector3.forward * controllerData.forwardImpulseForce + Vector3.up * controllerData.upwardImpulseForce, ForceMode.Impulse);
        }
    }

    private void Dive(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(State == ControllerState.FALLING || State == ControllerState.JUMPING)
        {
            State = ControllerState.DIVING;
        }

        if(State == ControllerState.SURFING)
        {
            State = ControllerState.DIVING;
            rb.AddForce(Vector3.down * controllerData.baseDivingForce, ForceMode.Impulse);
        }
    }

    private void DiveReleased(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(State == ControllerState.DIVING)
        {
            if(currentWaterBlock != null)
            {
                float currentDepth = currentWaterBlock.GetDepthAtPosition(transform.position, out _);

                //Kill vertical velocity before jumping
                rb.linearVelocity = HorizontalVelocity;

                State = ControllerState.SWIMMING;
            }
            else
            {
                State = ControllerState.FALLING;
            }
        }
    }

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.R))
        {
            transform.position = new Vector3(0, 1, 0);
            transform.rotation = Quaternion.identity;
            State = ControllerState.FALLING;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        thrust = inputs.thrust.action.ReadValue<float>();
        turn = inputs.turn.action.ReadValue<float>();
        brake = inputs.brake.action.ReadValue<float>();
        airControl = inputs.airControl.action.ReadValue<Vector2>();
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
            rb.linearVelocity = ClampYVelocity(Velocity, -controllerData.maxFallingSpeed, float.MaxValue);
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

        //IN WATER
        if(currentWaterBlock != null)
        {
            float currentDepth = currentWaterBlock.GetDepthAtPosition(transform.position, out _);
            if (currentDepth > maxDepth)
                maxDepth = currentDepth;
            
            //diving y force 
            if (State == ControllerState.DIVING)
            {
                if (currentDepth > 0 && rb.linearVelocity.y < 0)
                {
                    if (currentDepth < controllerData.baseDivingDepth)
                    {
                        //Drag on y velocity - the deeper the higher the drag 
                        rb.AddForce(new Vector3(0.0f, -rb.linearVelocity.y, 0.0f) * (currentDepth / controllerData.baseDivingDepth) * controllerData.underwaterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    }
                    else if (currentDepth > Mathf.Min(controllerData.maxDivingDepth, maxDivingDepth))
                    {
                        //Stop when hitting max depth
                        rb.AddForce(new Vector3(0.0f, -rb.linearVelocity.y, 0.0f), ForceMode.VelocityChange);
                        state = ControllerState.SWIMMING;
                    }
                }
            }

            if(State == ControllerState.SWIMMING)
            {
                rb.AddForce(Vector3.up * Mathf.Max(controllerData.minimumFloatingForce, maxDepth * controllerData.floatingForceMultiplier), ForceMode.Force);
            }
        }
        else //IN AIR
        {
            if(State == ControllerState.DIVING)
            {
                rb.AddForce(Vector3.down * controllerData.baseDivingForce);
                rb.linearVelocity = ClampYVelocity(Velocity, -controllerData.maxDivingFallingSpeed, float.MaxValue);
            }
        }

        //movement
        if (State == ControllerState.SURFING || State == ControllerState.SWIMMING)
        {
            Movement();
        }

        if(State == ControllerState.FALLING || State == ControllerState.DIVING)
        {
            AirControl();
        }

    }

    private void AirControl()
    {
        rb.linearDamping = 0;
        float inputMagnitude = Mathf.Max(Mathf.Abs(airControl.x), Mathf.Abs(airControl.y));
        Vector2 direction = airControl.normalized * inputMagnitude;
        float coeff = State == ControllerState.FALLING ? controllerData.fallingAirControl : controllerData.divingAirControl;

        rb.AddForce(transform.TransformDirection(new Vector3(direction.x, 0, direction.y)) * coeff * Time.fixedDeltaTime, ForceMode.VelocityChange);
        rb.linearVelocity = ClampHorizontalVelocity(Velocity, controllerData.maxAirControl);
    }

    private void Movement()
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
        maxDepth = 0;

        if(State == ControllerState.DIVING && Velocity.y < 0)
        {
            float speedRatio = Mathf.Clamp01((Mathf.Abs(Velocity.y) - controllerData.baseDivingForce) / (controllerData.maxDivingFallingSpeed - controllerData.baseDivingForce));
            maxDivingDepth = Mathf.Lerp(controllerData.baseDivingDepth, controllerData.maxDivingDepth, controllerData.VelocityToDivingDepthRatio.Evaluate(speedRatio));
        }

        if(State == ControllerState.FALLING || State == ControllerState.JUMPING)
        {
            State = ControllerState.SWIMMING;
            rb.linearVelocity = HorizontalVelocity;
            return;
        }

        Vector3 normal = (transform.position - collision.ClosestPoint(transform.position)).normalized;
        if (State == ControllerState.SURFING && Vector3.Dot(normal, Vector3.up) < 0.1f) 
        {
            //Enter from the side
            State = ControllerState.SWIMMING;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<WaterBlock>(out _))
        {
            currentWaterBlock = null;
            maxDepth = 0;
        }

        if (State == ControllerState.SURFING)
            return;

        Vector3 normal = (transform.position - collision.ClosestPoint(transform.position)).normalized;

        //keep diving
        if (State == ControllerState.DIVING && normal == Vector3.down)
            return;

        // else jump in normal direction
        if(State == ControllerState.SWIMMING || State == ControllerState.DIVING)
        {
            State = ControllerState.JUMPING;
            ExitWaterBlock(normal);
        }
    }

    private void ExitWaterBlock(Vector3 normal)
    {
        rb.AddForce(normal * controllerData.upwardImpulseForce * controllerData.jumpMultiplier, ForceMode.Impulse);
    }

    private Vector3 ClampYVelocity(Vector3 velocity, float minY, float maxY)
    {
        return new Vector3(velocity.x, Mathf.Clamp(velocity.y, minY, maxY), velocity.z);
    }

    private Vector3 ClampHorizontalVelocity(Vector3 velocity, float maxLength)
    {
        Vector3 toClamp = new Vector3(velocity.x, 0, velocity.z);
        Vector3 clamped = Vector3.ClampMagnitude(toClamp, maxLength);

        return new Vector3(clamped.x, velocity.y, clamped.z);
    }
}
