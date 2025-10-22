using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControllerState
{
    FALLING, 
    JUMPING,
    SURFING,
    DIVING, 
    SWIMMING,
    AIRRIDE,
    STOMP,
}

public class SimpleController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    public HoverBehaviour hoverBehaviour;
    [SerializeField]
    public BoostBehaviour boostBehaviour;
    [SerializeField]
    public StyleBehaviour styleBehaviour;

    private ControllerStats stats;
    private RailDetector railDetector;

    [Header("Parameters")]
    [SerializeField] public ControllerData controllerData;
    [SerializeField] private LayerMask raycastLayer;
    [SerializeField] private LayerMask targetRaycastLayer;

    //PLayer velocity
    public Vector3 Velocity => this.rb.linearVelocity;
    // Vector world space en local space par rapport a l'objet: permet de recup la vélocité horizontal peut importe la rotation
    private Vector3 TransformedVelocity => NormalContainer.InverseTransformVector(rb.linearVelocity);
    // Velocité horizontal
    public Vector3 HorizontalVelocity => NormalContainer.rotation * new Vector3(TransformedVelocity.x, 0f, TransformedVelocity.z);
    public Vector3 AngularVelocity => this.rb.angularVelocity;
    public float CurrentDepth => currentWaterBlock is null ? 0 : currentWaterBlock.GetDepthAtPosition(transform.position, out _);
    // Legacy water block behavior
    public float MaxDepth => currentWaterBlock is null ? 0 : maxDivingDepth;
    public bool IsDrifting => drifting;
    public int DriftDirection => driftDir;
    public Vector2 AirControlDirection => airControl;
    public bool InAirRail => currentAirRail != null;
    public bool OnRail => currentRail != null;
    public bool IsLocked => OnRail || InAirRail || forceLocked;
    private bool CanDrift => HorizontalVelocity.sqrMagnitude > controllerData.minSpeedToDrift * controllerData.minSpeedToDrift;
    private bool CanDriftBreak => HorizontalVelocity.sqrMagnitude < (controllerData.minSpeedToDriftBreak * controllerData.minSpeedToDriftBreak);
    private bool CanDash => (State == ControllerState.SURFING || State == ControllerState.FALLING) && 
                            currentDashTime > controllerData.dashTimer && 
                            (Time.time - lastDashTimestamp) > controllerData.dashCooldown;
    public int ConsecutiveDashCount => consecutiveDashCount;
    private Transform NormalContainer => hoverBehaviour.normalContainer;

    public ControllerState State {
        get
        { 
            return state; 
        }
        set
        {
            //Debug.Log(value);
            stateChanged.Invoke(state, value);
            state = value;
        }
    }

    private InputManager inputs;
    private float defaultDrag;
    float thrust, turn, brake = 0f;
    Vector2 airControl;
    Vector2 inputDirection;

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
    public Action<string> triggerAnim;
    public Action<string> enableBoolAnim;
    public Action<string> disableBoolAnim;
    public Action playTargetJumpParticles;
    public Action<bool> togglePlayerBodyVisual;
    public Action<string> straf;
    public Action<float> afterImageEffect;

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
        inputs.boost.action.performed += Stomp;
        inputs.jump.action.performed += Jump;
        inputs.jump.action.canceled += Jump;
        inputs.dash.action.performed += StyleDash;
        inputs.strafLeft.action.performed += Straf;
        inputs.strafRight.action.performed += Straf;

        //Components Setup
        hoverBehaviour.Initialize(controllerData, rb);
        styleBehaviour.Initialize(controllerData);
    }

    private void OnDisable()
    {
        inputs.boost.action.performed -= Boost;
        inputs.boost.action.performed -= Stomp;
        inputs.jump.action.performed -= Jump;
        inputs.jump.action.canceled -= Jump;
        inputs.dash.action.performed -= StyleDash;
        inputs.strafLeft.action.performed -= Straf;
        inputs.strafRight.action.performed -= Straf;
    }

    private void StyleDash(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if (IsDrifting && !controllerData.canDriftandDash)
            return;

        if(CanDash)
        {
            lastDashTimestamp = Time.time;
            consecutiveDashCount = Mathf.Clamp(consecutiveDashCount + 1, 0, controllerData.maxConsecutiveDashCount);

            if(State == ControllerState.SURFING)
                rb.AddForce(hoverBehaviour.normalContainer.forward * controllerData.dashForce, ForceMode.VelocityChange);

            styleBehaviour.StyleTrigger(hoverBehaviour.normalContainer.position, consecutiveDashCount);
            boostBehaviour.IncrementGauge(BoostAction.Dash);
            dash.Invoke(consecutiveDashCount);
        }
    }

    [HideInInspector] public float jumpChargeTimer { get; private set; }
    [HideInInspector] public bool chargesJump { get; private set; } = false;
    private void Jump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if (State == ControllerState.DIVING || State == ControllerState.SWIMMING || State == ControllerState.AIRRIDE)
            return;

        if (isCoyote)
        {
            //reset coyote
            currentCoyoteTime = 0;
            boostBehaviour.IncrementGauge(BoostAction.PerfectJump);
        }
        //CHARGE JUMP
        if (context.performed)
        {
            chargesJump = true;
            //print(chargesJump);
        }
        //RELEASE JUMP
        if (context.canceled)
        {
            chargesJump = false;
            float t = Mathf.Clamp01(jumpChargeTimer / controllerData.jumpChargeTime);
            float forceMultiplier = Mathf.Lerp(controllerData.jumpForceMultiplierMin, controllerData.jumpForceMultiplierMax, t);
            print("Force multiplier = " + forceMultiplier);
            //RESET TIMER AT END
            jumpChargeTimer = 0f;

            if (State == ControllerState.SURFING && jumpCount < 1)
            {
                // spin when surfing
                State = ControllerState.JUMPING;
                jumpCount++;
                rb.linearVelocity = hoverBehaviour.normalContainer.forward * HorizontalVelocity.magnitude;
                rb.AddForce((NormalContainer.up * controllerData.upwardImpulseForce * forceMultiplier) + (NormalContainer.forward * controllerData.forwardImpulseForce * forceMultiplier), ForceMode.VelocityChange);
                rb.linearDamping = controllerData.jumpDamping;

                // PLAY FMOD PLAYER ACTION JUMP SOUND
                PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.JUMP);

                //Play anim
                triggerAnim.Invoke("Spin");

                if (jumpRoutine != null)
                    StopCoroutine(jumpRoutine);
                jumpRoutine = StartCoroutine(JumpRoutine());
                return;
            }

            if (State == ControllerState.JUMPING || State == ControllerState.FALLING)
            {
                //Default in - air jump
                if (jumpCount <= 1)
                {
                    AirDash();
                }
                else if (jumpCount > 1) //boost gauge air-dash
                {
                    boostBehaviour.UseBoost(AirDash);

                }
            }
        }
    }

    private Coroutine jumpRoutine = null;
    private IEnumerator JumpRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        
        jumpRoutine = null;
    }

    Vector3 targetDashDirection = Vector3.zero;
    private void AirDash()
    {
        jumpCount = 2;
        State = ControllerState.JUMPING;
        //if (conditions pour target dash true)
        Collider[] colliders = Physics.OverlapSphere(hoverBehaviour.normalContainer.position, controllerData.targetDetectionRadius, controllerData.targetObjectsMask);
        // Check Valid target and choose valid Target

        List<Collider> validColliders = new List<Collider>();
        foreach (Collider target in colliders)
        {
            if (CameraTargetDetection.Instance.validTargets.Contains(target))
            {
                validColliders.Add(target);
            }
        }

        if (validColliders.Count > 0)
        {
            int index = 0;
            float distance = 0;
            Transform target = null;
            //Play anim
            triggerAnim.Invoke("TargetJump");
            playTargetJumpParticles.Invoke();
            if (validColliders.Count == 1)
            {
                    target = validColliders[0].transform;
            }
            else if (validColliders.Count > 1)
            {
                distance = Vector3.Distance(validColliders[0].transform.position, hoverBehaviour.normalContainer.position);
                for (int i = 1; i < validColliders.Count; i++)
                {
                    if (CameraTargetDetection.Instance.validTargets.Contains(validColliders[i]))
                    {
                        var dist = Vector3.Distance(validColliders[i].transform.position, hoverBehaviour.normalContainer.transform.position);
                        if (dist < distance)
                        {
                            index = i;
                            distance = dist;
                        }
                    }
                }
                target = validColliders[index].transform;
            }

            targetDashDirection = new Vector3(target.position.x - hoverBehaviour.normalContainer.transform.position.x,
                          target.position.y - hoverBehaviour.normalContainer.transform.position.y,
                          target.position.z - hoverBehaviour.normalContainer.transform.position.z);
            targetDashDirection = targetDashDirection.normalized;

            transform.forward = new Vector3(targetDashDirection.x, 0, targetDashDirection.z);
            rb.linearVelocity = targetDashDirection * HorizontalVelocity.magnitude;

            rb.AddForce(targetDashDirection * controllerData.targetBoostFactor, ForceMode.VelocityChange);

            if (jumpRoutine != null)
                StopCoroutine(jumpRoutine);
            jumpRoutine = StartCoroutine(JumpRoutine());

        }
        //DOUBLE JUMP
        else
        {
            rb.linearDamping = controllerData.doubleJumpDamping;
            triggerAnim.Invoke("TargetJump");
            playTargetJumpParticles.Invoke();

            ////REFACTO POUR EN FAIRE UN DASH ? BRO à LA VISION
            targetDashDirection = Camera.main.transform.forward.normalized;

            transform.forward = new Vector3(targetDashDirection.x, 0, targetDashDirection.z);
            rb.linearVelocity = targetDashDirection * HorizontalVelocity.magnitude;

            rb.AddForce(targetDashDirection * controllerData.doubleJumpForce, ForceMode.Impulse);

            if (jumpRoutine != null)
                StopCoroutine(jumpRoutine);
            jumpRoutine = StartCoroutine(JumpRoutine());

            ////Play anim
            //triggerAnim.Invoke("Spin");
            //Vector3 direction;
            //if (airControl.x != 0 || airControl.y != 0)
            //{
            //    direction = airControl.normalized;
            //    direction = transform.TransformDirection(new Vector3(direction.x, 0, direction.y));
            //}
            //else
            //{
            //    direction = transform.forward;
            //}

            ////transform.forward = direction;
            ////rb.linearVelocity = transform.forward * HorizontalVelocity.magnitude / 2;

            //rb.AddForce((NormalContainer.up * controllerData.upwardImpulseForce * 2 + direction * controllerData.upwardImpulseForce), ForceMode.VelocityChange);
            //rb.linearDamping = controllerData.jumpDamping;

            //// PLAY FMOD PLAYER ACTION JUMP SOUND
            //PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.JUMP);

            ////if (jumpRoutine != null)
            ////    StopCoroutine(jumpRoutine);
            ////jumpRoutine = StartCoroutine(JumpRoutine());
        }
    }

    private void Boost(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (State == ControllerState.SURFING)
        {
            boostBehaviour.UseBoost(() => Boost(controllerData.boostForce));
        }
    }

    private Coroutine strafRoutine = null;
    private IEnumerator StrafCooldownRoutine()
    {
        yield return new WaitForSeconds(controllerData.strafCooldown);

        strafRoutine = null;
    }

    private void Straf(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(strafRoutine == null)
        {
            strafRoutine = StartCoroutine(StrafCooldownRoutine());
            if (State == ControllerState.SURFING)
            {
                if(rb.linearVelocity.magnitude > controllerData.maxSpeed / 2)
                {
                    if (context.action.name == InputManager.Instance.strafLeft.action.name)
                    {
                        //rb.AddForce(-Camera.main.transform.right * controllerData.driftBoostForce, ForceMode.VelocityChange);
                        rb.linearVelocity = Vector3.zero;
                        rb.AddForce(-Camera.main.transform.right * controllerData.boostForce, ForceMode.VelocityChange);
                        straf.Invoke("StrafLeft");
                    }
                    else if (context.action.name == InputManager.Instance.strafRight.action.name)
                    {
                        //rb.AddForce(Camera.main.transform.right * controllerData.driftBoostForce, ForceMode.VelocityChange);
                        rb.linearVelocity = Vector3.zero;
                        rb.AddForce(Camera.main.transform.right * controllerData.boostForce, ForceMode.VelocityChange);
                        straf.Invoke("StrafRight");
                    }
                }
            }
        }
    }

    private Coroutine stompRoutine;
    private void Stomp(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(state == ControllerState.FALLING ||  state == ControllerState.JUMPING)
        {
            if (stompRoutine == null)
            {
                stompRoutine = StartCoroutine(StompRoutine(context));
            }
        }
    }

    private IEnumerator StompRoutine(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        State = ControllerState.STOMP;
        rb.linearVelocity = Vector3.zero;
        triggerAnim.Invoke("StompCharge");
        yield return new WaitForSeconds(controllerData.stompChargeTime);
        triggerAnim.Invoke("Stomp");
        afterImageEffect.Invoke(controllerData.stompAfterImageEffectTime);
        //play particle
        rb.AddForce(-hoverBehaviour.normalContainer.up * controllerData.stompForce, ForceMode.VelocityChange);
        
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.R))
        {
            Game.Instance.Respawn(out Vector3 position, out Quaternion rotation);
        }

        thrust = inputs.thrust.action.ReadValue<float>();
        turn = inputs.turn.action.ReadValue<float>();
        brake = inputs.brake.action.ReadValue<float>();
        airControl = inputs.airControl.action.ReadValue<Vector2>();

        //if (consecutiveDashCount != lastDashCount)
        //{
        //    lastDashCount = consecutiveDashCount;
        //    FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_StyleState, consecutiveDashCount);
        //}

        if (turn < 0)
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_TurnAngle, -1f);
        }
        else if(turn > 0)
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_TurnAngle, 1f);
        }
        else
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_TurnAngle, 0f);
        }
        if (Velocity.magnitude <= controllerData.maxSpeed)
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Speed, ValueMapping.Map(Velocity.magnitude, 0, 40, 0, 0.7f));
        }
        else
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Speed, 1f);
        }

        if(State == ControllerState.FALLING || State == ControllerState.JUMPING || State == ControllerState.AIRRIDE) 
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Flying, 1);
        }
        else
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Flying, 0);
        }

        if (State == ControllerState.SWIMMING)
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Underwater, 1);
        }
        else
        {
            FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Underwater, 0);
        }

        if (State == ControllerState.SURFING)
        {
            if(IsDrifting == true && hasDriftBoost == true)
            {
                FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Drift, 2);
            }
            else if (IsDrifting == true)
            {
                FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Drift, 1);
            }
            else
            {
                FmodGlobalParameters.instance.SetGlobalParameter(FmodGlobalParamName.G_Player_Drift, 0);
            }
        }
    }

    bool hasHit = false;
    float xRotation = 0f;
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

        //Charging a Jump
        if (chargesJump)
        {
            jumpChargeTimer += Time.deltaTime;
            jumpChargeTimer = Mathf.Clamp(jumpChargeTimer, 0, controllerData.jumpChargeTime);
            print(jumpChargeTimer);
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

        //Stomping to Surfing
        if (State == ControllerState.STOMP)
        {
            rb.AddForce(-hoverBehaviour.normalContainer.up * controllerData.stompAccelForce, ForceMode.Acceleration);
            if (hasHit)
            {
                State = ControllerState.SURFING;
                Boost(controllerData.boostForce);
                stompRoutine = null;
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
        if (State == ControllerState.SURFING)
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

    public Vector3 direction;
    private void Movement()
    {
        float speed = controllerData.acceleration;

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        //camForward.y = 0;
        //camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        inputDirection = inputs.moveDirection.action.ReadValue<Vector2>();
        direction = (camForward * inputDirection.y + camRight * inputDirection.x).normalized;

        //if (thrust > 0.0 && HorizontalVelocity.sqrMagnitude < (controllerData.maxSpeed * controllerData.maxSpeed))
        //{
        //    direction = new Vector3(inputDirection.x, 0f, inputDirection.y);
        //}
        //else
        //{
        //    //direction = inputDirection * speed * controllerData.overSpeedCoeff;
        //    direction = new Vector3(inputDirection.x, 0f, inputDirection.y);
        //}

        Vector3 targetDir = direction;

        if (targetDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            Quaternion deltaRot = targetRot * Quaternion.Inverse(rb.rotation);

            deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;

            Vector3 torque = axis * angle * Mathf.Deg2Rad * controllerData.rotationTorque;
            rb.AddTorque(torque, ForceMode.Acceleration);
        }

        float speedRatio = GetSpeedRatio();
        float steer = stats.GetSteering(speedRatio, turn, false);
        float steeringVelocity = Vector3.Dot(transform.right, Velocity);
        float desiredVelocityChange = -steeringVelocity * stats.GetGrip() * Time.fixedDeltaTime;

        rb.AddForce(direction * speed, ForceMode.Acceleration);

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
        afterImageEffect.Invoke(controllerData.boostAfterImageEffectDuration);
        boost.Invoke();
        triggerAnim.Invoke("Boost");
    }

    private void DriftBoost()
    {
        if (currentDriftTime > controllerData.driftBoostTimer)
        {
            Boost(controllerData.boostForce);
            boostBehaviour.IncrementGauge(BoostAction.BoostedDrift);
        }
    }

    public void StopByTargetImpact(GameObject target)
    {
        togglePlayerBodyVisual.Invoke(false);

        ForceLock(true);

        if(target.GetComponent<JumpTarget>() != null)
        {
            transform.position = target.transform.position;
            target.GetComponent<JumpTarget>().StartLaunchCoroutine();
        }

        //rb.AddForce(hoverBehaviour.normalContainer.up * controllerData.targetBounceForce, ForceMode.VelocityChange);
        //ResetJump();
    }

    public void PropelledByTarget(Transform target, float propulsionForce)
    {
        if(forceLocked == true)
        {
            ForceLock(false);
        }
        transform.rotation = new Quaternion(0, target.transform.rotation.y, 0, target.transform.rotation.w);
        rb.AddForce(target.forward * propulsionForce, ForceMode.VelocityChange);
        ResetJump();
    }

    public void BounceOnTarget(float propulsionForce)
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce((hoverBehaviour.normalContainer.forward * propulsionForce) + (hoverBehaviour.normalContainer.up * propulsionForce / 1.5f), ForceMode.VelocityChange);
        ResetJump();
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