using DG.Tweening;
using System;
using System.Collections;
using System.Diagnostics.Contracts;
using UnityEngine;
using FMODUnity;
using System.Runtime.CompilerServices;

public enum ControllerState
{
    FALLING, 
    JUMPING,
    SURFING,
    DIVING, 
    SWIMMING,
    AIRRIDE,
}

public class SimpleController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private HoverBehaviour hoverBehaviour;
    [SerializeField]
    public BoostBehaviour boostBehaviour;

    private ControllerStats stats;
    private RailDetector railDetector;

    [Header("Parameters")]
    [SerializeField] public ControllerData controllerData;
    [SerializeField] private LayerMask raycastLayer;

    public Vector3 Velocity => this.rb.linearVelocity;
    private Vector3 TransformedVelocity => NormalContainer.InverseTransformVector(rb.linearVelocity);
    public Vector3 HorizontalVelocity => NormalContainer.rotation * new Vector3(TransformedVelocity.x, 0f, TransformedVelocity.z);
    public Vector3 AngularVelocity => this.rb.angularVelocity;
    public float CurrentDepth => currentWaterBlock is null ? 0 : currentWaterBlock.GetDepthAtPosition(transform.position, out _);
    public float MaxDepth => currentWaterBlock is null ? 0 : maxDivingDepth;
    public bool IsDrifting => drifting;
    public int DriftDirection => driftDir;
    public Vector2 AirControlDirection => airControl;
    public bool InAirRail => currentAirRail != null;
    public bool OnRail => currentRail != null;
    public bool IsLocked => OnRail || InAirRail || forceLocked;
    private bool CanDrift => HorizontalVelocity.sqrMagnitude > controllerData.minSpeedToDrift * controllerData.minSpeedToDrift;
    private bool CanDriftBreak => HorizontalVelocity.sqrMagnitude < (controllerData.minSpeedToDriftBreak * controllerData.minSpeedToDriftBreak);
    private bool CanDash => State == ControllerState.SURFING && currentDashTime > controllerData.dashTimer && (Time.time - lastDashTimestamp) > controllerData.dashCooldown;
    private Transform NormalContainer => hoverBehaviour.normalContainer;

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
    private AirRail currentAirRail;
    private Rail currentRail;
    private float maxDivingDepth;
    private float maxDepth;
    private int jumpCount;
    private bool drifting;
    private int driftDir;
    private bool isCoyote => currentCoyoteTime > 0;
    private float currentCoyoteTime;
    private float currentDriftTime;
    private float currentDashTime;
    private float lastDashTimestamp = 0f;
    private int consecutiveDashCount;
    private bool hasDriftBoost;
    private bool forceLocked;

    public Action<ControllerState, ControllerState> stateChanged;
    public Action<AirRail> enterAirRail;
    public Action<AirRail> exitAirRail;
    public Action<int, bool, bool> updateDrift;
    public Action boost;
    public Action<Transform> updateRaceTarget;
    public Action enterRail;
    public Action exitRail;
    public Action<int> dash;

    private void Awake()
    {
        stats = new ControllerStats(this, this.controllerData);
        railDetector = GetComponentInChildren<RailDetector>();
    }

    private void Start()
    {
        inputs = InputManager.Instance;
        defaultDrag = rb.linearDamping;
        State = ControllerState.FALLING;

        inputs.boost.action.performed += Boost;
        inputs.jump.action.performed += Jump;
        inputs.drift.action.performed += Drift;
        inputs.drift.action.canceled += DriftReleased;
        inputs.dash.action.performed += Dash;

        //Components Setup
        hoverBehaviour.Initialize(controllerData, rb);
    }

    private void OnDisable()
    {
        inputs.boost.action.performed -= Boost;
        inputs.jump.action.performed -= Jump;
        inputs.drift.action.performed -= Drift;
        inputs.drift.action.canceled -= DriftReleased;
    }

    private void SetDrift(int dir, bool drifting, bool boost = false)
    {
        this.drifting = drifting;
        driftDir = dir;

        if (drifting == false)
        {
            currentDriftTime = 0;
            hasDriftBoost = false;
        }

        updateDrift.Invoke(dir, drifting, boost);
    }

    private void Dash(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if (IsDrifting && !controllerData.canDriftandDash)
            return;

        if(CanDash)
        {
            lastDashTimestamp = Time.time;
            consecutiveDashCount = Mathf.Clamp(consecutiveDashCount + 1, 0, controllerData.maxConsecutiveDashCount);

            rb.AddForce(hoverBehaviour.normalContainer.forward * controllerData.dashForce, ForceMode.VelocityChange);
            boostBehaviour.IncrementGauge(BoostAction.Dash);
            dash.Invoke(consecutiveDashCount);
        }
    }

    private void Drift(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if (State == ControllerState.SURFING)
        {
            if (turn == 0 || CanDrift == false)
                return;
            
            SetDrift(turn > 0 ? 1 : -1, true);
        }
        
        //Backflip
        if(state == ControllerState.AIRRIDE)
        {
            rb.linearVelocity = HorizontalVelocity;
        }
    }

    private void DriftReleased(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if(IsDrifting && State == ControllerState.SURFING)
        {
            DriftBoost();
        }

        SetDrift(0, false);
    }

    private void Jump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if (State == ControllerState.DIVING || State == ControllerState.SWIMMING)
            return;

        if (isCoyote)
        {
            //reset coyote
            currentCoyoteTime = 0;
            boostBehaviour.IncrementGauge(BoostAction.PerfectJump);
        }

        if(State == ControllerState.SURFING && jumpCount < 1)
        {
            // spin when surfing
            State = ControllerState.JUMPING;
            jumpCount++;
            rb.linearVelocity = hoverBehaviour.normalContainer.forward * HorizontalVelocity.magnitude;
            rb.AddForce(NormalContainer.up * controllerData.upwardImpulseForce, ForceMode.VelocityChange);
            rb.linearDamping = controllerData.jumpDamping;

            // PLAY FMOD PLAYER ACTION JUMP SOUND
            PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.JUMP);

            if (jumpRoutine != null)
                StopCoroutine(jumpRoutine);
            jumpRoutine = StartCoroutine(JumpRoutine());
            return;
        }

        if(State == ControllerState.JUMPING || State == ControllerState.FALLING)
        {
            //Default in - air jump
            if(jumpCount <= 1)
            {
                AirDash();
            }
            else if(jumpCount > 1) //boost gauge air-dash
            {
                boostBehaviour.UseBoost(AirDash);
            }
        }
    }

    private Coroutine jumpRoutine = null;
    private IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        jumpRoutine = null;
    }

    private void AirDash()
    {
        jumpCount = 2;
        State = ControllerState.JUMPING;
        Vector3 direction;
        if (airControl.x != 0 || airControl.y != 0)
        {
            direction = airControl.normalized;
            direction = transform.TransformDirection(new Vector3(direction.x, 0, direction.y));
        }
        else
        {
            direction = transform.forward;
        }

        transform.forward = direction;
        rb.linearVelocity = transform.forward * HorizontalVelocity.magnitude;

        rb.AddForce(NormalContainer.up * controllerData.upwardImpulseForce, ForceMode.VelocityChange);
        rb.linearDamping = controllerData.jumpDamping;

        // PLAY FMOD PLAYER ACTION JUMP SOUND
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.JUMP);
        
        if (jumpRoutine != null)
            StopCoroutine(jumpRoutine);
        jumpRoutine = StartCoroutine(JumpRoutine());

    }

    private void Boost(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (State == ControllerState.SURFING)
        {
            boostBehaviour.UseBoost(() => Boost(controllerData.driftBoostForce));
        }
    }

    //Coroutine airDiveRoutine;
    //private void Dive(UnityEngine.InputSystem.InputAction.CallbackContext context)
    //{
    //    if (IsLocked || State == ControllerState.DIVING)
    //        return;

    //    if (airDiveRoutine != null)
    //        return;

    //    if(State == ControllerState.FALLING || State == ControllerState.JUMPING)
    //    {
    //        if (rb.linearVelocity.y > 0)
    //            rb.linearVelocity = HorizontalVelocity;

    //        rb.linearVelocity = Vector3.zero;
    //        airDiveRoutine = StartCoroutine(AirdiveRoutine());
    //    }

    //    if(State == ControllerState.AIRRIDE)
    //    {
    //        rb.linearVelocity = Vector3.zero;
    //        airDiveRoutine = StartCoroutine(AirdiveRoutine());
    //    }

    //    if(State == ControllerState.SURFING)
    //    {
    //        State = ControllerState.DIVING;
    //        rb.AddForce(Vector3.down * controllerData.baseDivingForce, ForceMode.VelocityChange);
    //    }
    //}

    //private void DiveReleased(UnityEngine.InputSystem.InputAction.CallbackContext context)
    //{
    //    if (IsLocked)
    //        return;

    //    if(State == ControllerState.DIVING)
    //    {
    //        if(currentWaterBlock != null)
    //        {
    //            float currentDepth = currentWaterBlock.GetDepthAtPosition(transform.position, out _);

    //            //Kill vertical velocity before jumping
    //            rb.linearVelocity = HorizontalVelocity;

    //            State = ControllerState.SWIMMING;
    //        }
    //    }
    //}

    //private IEnumerator AirdiveRoutine()
    //{
    //    yield return new WaitForSeconds(0.5f);
    //    rb.AddForce(Vector3.down * controllerData.baseDivingForce, ForceMode.VelocityChange);
    //    State = ControllerState.DIVING;
    //    airDiveRoutine = null;
    //}

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.R))
        {
            Game.Instance.Respawn(out Vector3 position, out Quaternion rotation);
        }

        thrust = inputs.thrust.action.ReadValue<float>();
        turn = inputs.turn.action.ReadValue<float>();
        brake = inputs.brake.action.ReadValue<float>();
        airControl = inputs.airControl.action.ReadValue<Vector2>();
    }

    bool hasHit = false;
    private void FixedUpdate()
    {
        hasHit = Physics.Raycast(hoverBehaviour.normalContainer.position, -hoverBehaviour.normalContainer.up, out RaycastHit info, controllerData.hoverRaycastLength, raycastLayer.value);

        if (OnRail)
        {
            if(false == currentRail.Progress(Time.fixedDeltaTime, out Vector3 nextPos, out Vector3 normal, out Vector3 direction))
            {
                currentRail = null;
                transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
                rb.isKinematic = false;
                rb.AddForce(direction * 50, ForceMode.VelocityChange);
                exitRail.Invoke();
                railDetector.ExitRail();
            }
            else
            {
                transform.position = nextPos;
                transform.forward = direction;
            }

            return;
        }

        if(InAirRail)
        {
            rb.linearVelocity = currentAirRail.direction.forward * currentAirRail.rideForce;
            
            if (currentAirRail.InAirRail(transform.position) == false)
            {
                exitAirRail.Invoke(currentAirRail);
                currentAirRail = null;
            }
            return;
        }

        if(IsDrifting)
        {
            if(State != ControllerState.SURFING || CanDriftBreak)
            {
                //Stop drifting
                SetDrift(0, false);
            }

            currentDriftTime += Time.fixedDeltaTime;
            if(currentDriftTime > controllerData.driftBoostTimer && hasDriftBoost == false)
            {
                hasDriftBoost = true;
                SetDrift(driftDir, true, true);
            }
        }

        //Falling to Surfing
        if (State == ControllerState.FALLING)
        {
            if (hasHit)
            {
                State = ControllerState.SURFING;
                ResetJump();
            }
        }

        //Jumping / AirRide to Falling
        if ((State == ControllerState.JUMPING || State == ControllerState.AIRRIDE) && 
            (Vector3.Dot(NormalContainer.up, rb.linearVelocity.normalized) < 0 || hasHit) && 
            currentWaterBlock == null && 
            jumpRoutine == null)
        {
            State = ControllerState.FALLING;
        }

        //Jumping to AirRide
        if(State == ControllerState.JUMPING)
        {
            if(Velocity.y > controllerData.airRideVelocityThreshold)
            {
                State = ControllerState.AIRRIDE;
                rb.linearDamping = defaultDrag;
            }
        }

        //Apply gravity
        if (State == ControllerState.JUMPING ||
            State ==  ControllerState.FALLING ||
            State == ControllerState.AIRRIDE)
        {
            float force = controllerData.gravity;
            if (State == ControllerState.AIRRIDE)
                force *= controllerData.airRideGravityScale;

            rb.AddForce(Vector3.down * force, ForceMode.Acceleration);
            rb.linearVelocity = ClampYVelocity(Velocity, -controllerData.maxFallingSpeed, float.MaxValue);
        }

        //Hover on water
        if (State == ControllerState.SURFING)
        {
            if(hasHit)
            {
                hoverBehaviour.Hover(info, Time.fixedDeltaTime);

                if (HorizontalVelocity.sqrMagnitude > controllerData.minSpeedToDash)   
                    currentDashTime += Time.deltaTime;
                else
                    currentDashTime = 0f;

                //reset dash counter if over threshold
                if ((Time.time - lastDashTimestamp) > controllerData.dashCooldown + controllerData.dashTimeThreshold)
                    consecutiveDashCount = 0;

            }
            else
            {
                currentCoyoteTime += Time.fixedDeltaTime;
                if(currentCoyoteTime > controllerData.coyoteTime)
                {
                    State = ControllerState.FALLING;
                    currentCoyoteTime = 0; 
                }
            }
        }
        else
        {
            hoverBehaviour.ResetRotation(Time.fixedDeltaTime);
        }

        //IN WATER
        if (currentWaterBlock != null)
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
                        State = ControllerState.SWIMMING;
                    }
                }
            }

            if (State == ControllerState.SWIMMING)
            {
                float force = Mathf.Min(controllerData.maximumFloatingForce, Mathf.Max(controllerData.minimumFloatingForce, maxDepth * controllerData.floatingForceMultiplier));
                rb.AddForce(Vector3.up * force, ForceMode.Acceleration);
            }
        }
        else //IN AIR
        {
            if (State == ControllerState.DIVING)
            {
                rb.AddForce(Vector3.down * controllerData.baseDivingForce, ForceMode.Acceleration);
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

        if(State == ControllerState.FALLING || State == ControllerState.JUMPING)
        {
            if (hasHit)
                return;

            Vector2 direction = this.airControl.normalized;
            Vector3 airControl = transform.TransformDirection(new Vector3(direction.x, 0, direction.y));

            if(hoverBehaviour.CanLockToSurface(airControl, out Vector3 surfaceNormal, out Vector3 hitPoint))
            {
                NormalContainer.up = surfaceNormal;
                NormalContainer.Rotate(0, transform.eulerAngles.y, 0);
                State = ControllerState.SURFING;
                ResetJump();
            }
        }

    }

    private void AirControl()
    {
        rb.linearDamping = 0;
        float inputMagnitude = Mathf.Max(Mathf.Abs(this.airControl.x), Mathf.Abs(this.airControl.y));
        Vector2 direction = this.airControl.normalized * inputMagnitude;
        float coeff = State == ControllerState.FALLING ? controllerData.fallingAirControl : controllerData.divingAirControl;

        if(State == ControllerState.FALLING && turn != 0)
            rb.AddTorque(new Vector3(0,Mathf.Sign(turn) * controllerData.airControlRotationSpeed ,0), ForceMode.Acceleration);

        Vector3 airControl = transform.TransformDirection(new Vector3(direction.x, 0, direction.y));
        var dot = Vector3.Dot(airControl, HorizontalVelocity);
        if (dot > 0 && dot > controllerData.maxAirControl)
            return;

        rb.AddForce(airControl * coeff * Time.fixedDeltaTime, ForceMode.VelocityChange);
    }

    private void Movement()
    {
        float speed = controllerData.acceleration;

        float forward = 0.0f;
        if (thrust > 0.0 && HorizontalVelocity.sqrMagnitude < (controllerData.maxSpeed * controllerData.maxSpeed))
        {
            forward = thrust * speed;
        }
        else
        {
            forward = thrust * speed * controllerData.overSpeedCoeff;
        }


        float speedRatio = GetSpeedRatio();
        float steer = stats.GetSteering(speedRatio, turn, false);
        float steeringVelocity = Vector3.Dot(transform.right, Velocity);
        float desiredVelocityChange = -steeringVelocity * stats.GetGrip() * Time.fixedDeltaTime;

        //Apply forces (grip - thrust - steer)
        rb.AddForce(hoverBehaviour.normalContainer.right * desiredVelocityChange, ForceMode.VelocityChange);
        rb.AddForce(hoverBehaviour.normalContainer.forward * forward, ForceMode.Acceleration);

        if (IsDrifting)
        {
            Steer(stats.GetSteering(speedRatio, turn, true, driftDir));
        }
        else
        {
            Steer(steer);
        }

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

    private void Steer(float steerAmount)
    {
        rb.AddTorque(new Vector3(0.0f, steerAmount, 0.0f), ForceMode.Acceleration);
    }
    
    private void ExitWaterBlock(Vector3 normal)
    {
        rb.AddForce(normal * controllerData.upwardImpulseForce * controllerData.jumpMultiplier, ForceMode.VelocityChange);
        rb.linearDamping = controllerData.jumpDamping;
        jumpCount++;
    }
    
    private void Boost(float force)
    {
        rb.AddForce(hoverBehaviour.normalContainer.forward * force, ForceMode.VelocityChange);
        boost.Invoke();
    }

    private void DriftBoost()
    {
        if (currentDriftTime > controllerData.driftBoostTimer)
        {
            Boost(controllerData.driftBoostForce);
            boostBehaviour.IncrementGauge(BoostAction.BoostedDrift);
        }
    }

    public void EnterAirRail(AirRail rail)
    {
        if (State != ControllerState.SURFING || currentAirRail != null)
            return;

        enterAirRail.Invoke(rail);
        boost.Invoke();
        currentAirRail = rail;
    }

    public bool EnterRail(Rail rail)
    {
        if (OnRail)
            return false;

        currentRail = rail;
        rail.EnterRail(transform.position, Velocity);
        rb.isKinematic = true;
        boost.Invoke();
        enterRail.Invoke();
        return true;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (false == collision.gameObject.TryGetComponent<WaterBlock>(out WaterBlock waterBlock))
        {
            return;
        }

        currentWaterBlock = waterBlock;
        maxDepth = 0;

        ResetJump();

        if (State == ControllerState.DIVING && Velocity.y < 0)
        {
            float speedRatio = Mathf.Clamp01((Mathf.Abs(Velocity.y) - controllerData.baseDivingForce) / (controllerData.maxDivingFallingSpeed - controllerData.baseDivingForce));
            maxDivingDepth = Mathf.Lerp(controllerData.baseDivingDepth, controllerData.maxDivingDepth, controllerData.VelocityToDivingDepthRatio.Evaluate(speedRatio));
        }

        if (State == ControllerState.FALLING || State == ControllerState.JUMPING || State == ControllerState.AIRRIDE)
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
        if (collision.gameObject.TryGetComponent<WaterBlock>(out WaterBlock block) && block == currentWaterBlock)
        {
            currentWaterBlock = null;
            maxDepth = 0;
        }
        else
            return;

        if (State == ControllerState.SURFING)
            return;

        ResetJump();

        Vector3 normal = (transform.position - collision.ClosestPoint(transform.position)).normalized;

        //keep diving
        if (State == ControllerState.DIVING && normal == Vector3.down)
            return;

        // else jump in normal direction
        if (State == ControllerState.SWIMMING || State == ControllerState.DIVING)
        {
            State = ControllerState.JUMPING;
            ExitWaterBlock(normal);
        }
    }

    private Vector3 ClampYVelocity(Vector3 velocity, float minY, float maxY)
    {
        return new Vector3(velocity.x, Mathf.Clamp(velocity.y, minY, maxY), velocity.z);
    }

    public void ForcePosition(Transform transform) => ForcePosition(transform.position, transform.rotation);

    public void ForcePosition(Vector3 position, Quaternion rotation, bool resetVelocity = true, ControllerState forcedState = ControllerState.FALLING)
    {
        State = forcedState;
        if(resetVelocity)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = position;
        transform.rotation = rotation;
        transform.rotation = new Quaternion(0, transform.rotation.y, 0, transform.rotation.w);
    }

    private float GetSpeedRatio()
    {
        var ratio = HorizontalVelocity.sqrMagnitude / (controllerData.maxSpeed * controllerData.maxSpeed);
        return Mathf.Clamp01(ratio);
    }

    private Coroutine m_LockCoroutine;
    private IEnumerator LockRoutine(float duration)
    {
        ForceLock(true);
        gameObject.GetComponent<Collider>().enabled = false;
        yield return new WaitForSeconds(duration);
        ForceLock(false);
        gameObject.GetComponent<Collider>().enabled = true;
        m_LockCoroutine = null;
    }
    public void LockPlayerForDuration(float duration)
    {
        if (m_LockCoroutine != null)
        {
            Debug.LogError("Player already locked");
            return;
        }
        m_LockCoroutine = StartCoroutine(LockRoutine(duration));
    }

    public void ForceLock(bool lockController)
    {
        forceLocked = lockController;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = lockController;
    }

    private void ResetJump() => jumpCount = 0;

    public void UpdateRaceTarget(Transform target) => updateRaceTarget.Invoke(target);
}