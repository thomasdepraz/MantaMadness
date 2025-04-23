using System;
using System.Linq;
using UnityEngine;

public enum ControllerState
{
    FALLING, 
    JUMPING,
    SURFING,
    DIVING
}

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
    float thrust, turn, brake = 0f;

    private ControllerState state;
    private WaterBlock currentWaterBlock;

    private void Start()
    {
        inputs = InputManager.Instance;
        defaultDrag = rb.linearDamping;
        state = ControllerState.FALLING;

        inputs.dive.action.performed += Dive;
        inputs.dive.action.canceled += DiveReleased;
        inputs.jump.action.performed += Jump;
    }


    private void Jump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
    }

    private void Dive(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(state == ControllerState.SURFING)
        {
            state = ControllerState.DIVING;
            rb.AddForce(Vector3.down * controllerData.baseDivingForce, ForceMode.Impulse);
        }
    }

    private void DiveReleased(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(state == ControllerState.DIVING && currentWaterBlock != null)
        {
            float currentDepth = currentWaterBlock.GetDepthAtPosition(transform.position, out _);
            rb.AddForce(Vector3.up * currentDepth * controllerData.jumpMultiplier, ForceMode.Impulse);

            state = ControllerState.JUMPING;
        }
    }

    private void Update()
    {
        thrust = inputs.thrust.action.ReadValue<float>();
        turn = inputs.turn.action.ReadValue<float>();
        brake = inputs.brake.action.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        bool hasHit = Physics.Raycast(transform.position, -transform.up, out RaycastHit info, controllerData.hoverRaycastLength);

        if (state == ControllerState.FALLING)
        {
            if (hasHit)
                state = ControllerState.SURFING;
        }

        if(state == ControllerState.JUMPING && rb.linearVelocity.y < 0)
        {
            state = ControllerState.FALLING;
            Debug.Log("Start Falling");
        }

        //Apply gravity
        if (state == ControllerState.JUMPING ||
            state ==  ControllerState.FALLING ||
            state == ControllerState.SURFING)
        {
            rb.AddForce(Vector3.down * 9.8f);
        }

        //Hover on water
        if (state == ControllerState.SURFING)
        {
            if(hasHit)
            {
                SurfHover(info);
            }
            else
            {
                state = ControllerState.FALLING;
                Debug.Log("Fall off block top");
            }
        }

        //diving y force 
        if(state == ControllerState.DIVING && currentWaterBlock != null)
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
        if(state == ControllerState.SURFING || state == ControllerState.DIVING)
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

        if(state == ControllerState.FALLING)
        {
            float currentDepth = currentWaterBlock.GetDepthAtPosition(transform.position, out _);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * currentDepth * controllerData.jumpMultiplier, ForceMode.Impulse);

            state = ControllerState.JUMPING;
        }
    }


    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<WaterBlock>(out _))
            currentWaterBlock = null;

        if (state != ControllerState.JUMPING)
        {
            state = ControllerState.FALLING;
            Debug.Log("Fall of block side");
        }
    }
}
