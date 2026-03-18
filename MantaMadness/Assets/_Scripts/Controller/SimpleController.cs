using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public enum ControllerState
{
    FALLING, 
    JUMPING,
    SURFING,
    DIVING, 
    SWIMMING,
    AIRRIDE,
    STOMP,
    BOOSTJUMP,
    DRIFT,
    DIALOG,
    BUMP,
    RAIL,
    LEDGEGRAB,
}

public enum ControllerAbility
{
    DOUBLEJUMP,
    CHARGEBOOST,
    STOMP,
    LAVARESIST,
    ALIEN,
    GRIND,
    CAT,
}

public enum ActionWindowType
{
    None,
    StompLand,
    StompBuildup,
    PerfectSpin,
    PerfectRail,
}

public enum ActionWindowResult
{
    Default,
    Jump,
    Spin,
    SuperSpin,
}

public class ActionWindow
{
    public ActionWindowType Type;

    private float endTime;
    private bool isConsumed;

    public System.Action OnStart;
    public System.Action OnEnd;
    public System.Action<ActionWindowResult> OnSuccess;
    public System.Action OnFail;
    public System.Action OnCancel;

    public bool IsActive => !isConsumed && Time.time <= endTime;

    public void Start(float duration)
    {
        endTime = Time.time + duration;
        isConsumed = false;
        OnStart?.Invoke();
    }

    public void Update()
    {
        if (isConsumed)
            return;

        if (Time.time > endTime)
        {
            Fail();
        }
    }

    public void Success(ActionWindowResult result)
    {
        if (isConsumed)
            return;

        isConsumed = true;
        OnSuccess?.Invoke(result);
        OnEnd?.Invoke();
    }

    public void Fail()
    {
        if (isConsumed)
            return;

        isConsumed = true;
        OnFail?.Invoke();
        OnEnd?.Invoke();
    }

    public void Cancel()
    {
        if (isConsumed)
            return;

        isConsumed = true;
        OnCancel?.Invoke();
        OnEnd?.Invoke();
    }

    public bool IsFinished => isConsumed;
}

public class SimpleController : MonoBehaviour, IDataPersistence
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    public HoverBehaviour hoverBehaviour;
    [SerializeField]
    public BoostBehaviour boostBehaviour;
    [SerializeField]
    public StyleBehaviour styleBehaviour;
    [SerializeField]
    public SpinBehavior spinBehaviour;

    private ControllerStats stats;
    private RailDetector railDetector;

    [Header("Parameters")]
    [SerializeField] public ControllerData controllerData;
    [SerializeField] private LayerMask waterRaycastLayer;
    [SerializeField] private LayerMask obstacleRaycastLayer;
    [SerializeField] private LayerMask defaultRaycastLayer;
    [SerializeField] private LayerMask targetRaycastLayer;

    [Header("Player Abilities")]
    [SerializeField] public bool doubleJumpAbility { get; private set; }
    [SerializeField] public bool chargeBoostAbility { get; private set; }
    [SerializeField] public bool stompAbility { get; private set; }
    [SerializeField] public bool lavaResistanceAbility { get; private set; }
    [SerializeField] public bool alienAntennasAbility { get; private set; }
    [SerializeField] public bool grindAbility { get; private set; }
    [SerializeField] public bool catAbility { get; private set; }

    [Header("Ledge Grab")]
    [SerializeField] private float ledgeHeight = 1.5f;
    [SerializeField] private float ledgeForwardOffset = 0.3f;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private LayerMask waterLayer;

    [Header("Ledge Grab - 360")]
    [SerializeField] private float ledgeWallProbeRadius = 0.35f; // rayon du spherecast
    [SerializeField] private float ledgeWallProbeDistance = 0.75f; // portée

    private Vector3 savedPreLedgeUp = Vector3.up;
    private Vector3 savedPreLedgeForward = Vector3.forward;
    private Vector3 savedLedgeExitForward = Vector3.forward; // basé sur ledgeNormal

    private Vector3 ledgePoint;
    private Vector3 ledgeNormal;

    float lastLedgeGrabTime;
    [SerializeField] float ledgeCooldown = 0.3f;


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
    public bool IsDrifting => drifting || forwardDrifting;
    public float DriftDirection => driftDir;
    public Vector2 AirControlDirection => airControl;
    public bool InAirRail => currentAirRail != null;
    public bool OnRail => currentRail != null;
    public bool OnWaterFall => currentWaterFall != null;
    public bool IsLocked => OnRail || InAirRail || forceLocked;
    public bool IsSpinning => isSpinning || isRailSpinning || isSuperSpinning;
    public Rail CurrentRail => currentRail;

    private bool CanDrift => HorizontalVelocity.sqrMagnitude > controllerData.minSpeedToDrift * controllerData.minSpeedToDrift;
    //private bool CanDriftBreak => HorizontalVelocity.sqrMagnitude < (controllerData.minSpeedToDriftBreak * controllerData.minSpeedToDriftBreak);
    private bool CanDash => (State == ControllerState.SURFING || State == ControllerState.FALLING) && 
                            currentDashTime > controllerData.dashTimer && 
                            (Time.time - lastDashTimestamp) > controllerData.styleCooldown;
    private bool hasResetCam => (State == ControllerState.SURFING && IsDrifting);

    public int ConsecutiveDashCount => consecutiveDashCount;
    private Transform NormalContainer => hoverBehaviour.normalContainer;

    public bool CanInteract => interact;

    public Vector3 grindOffset = new Vector3(0, 2f, 0);

    [Header("Rail")]
    [SerializeField] private float railReenterCooldown = 0.25f;
    private float lastRailExitTime = -999f;

    [Header("Rail Transfer")]
    [SerializeField] float railTransferDistance = 6f;
    [SerializeField] float railTransferRayOffset = 1.2f;
    [SerializeField] LayerMask railLayer;

    Rail leftRailCandidate;
    Rail rightRailCandidate;

    Vector3 leftRailPoint;
    Vector3 rightRailPoint;

    public System.Action<Vector3> showRailTransferLeft;
    public System.Action<Vector3> showRailTransferRight;
    public System.Action hideRailTransfer;


    public ControllerState State {
        get
        { 
            return state; 
        }
        set
        {
            if (state == value)
                return;

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
    private Rail lastRail;
    private WaterFall currentWaterFall;
    private float maxDivingDepth;
    private float maxDepth;
    private int jumpCount;
    private bool drifting;
    private bool forwardDrifting;
    private float driftDir;
    private bool isCoyote => currentCoyoteTime > 0;
    private float currentCoyoteTime;
    private float currentDriftTime;
    private float currentDashTime;
    private float currentSpinTime;
    private float lastDashTimestamp = 0f;
    private int consecutiveDashCount;
    private bool hasDriftBoost;
    private bool hasSpinBoost;
    private bool forceLocked;
    private bool actionWindowLocked;
    private bool interact;
    public bool railLock;
    private bool isSpinning;
    private bool isSuperSpinning;
    private bool hasAirSpin;
    private bool spinBufferedFromJump;
    private bool spinBufferedFromStompBuildup;
    private bool isRailSpinning;
    private bool stompBuildupWindowEnded;

    private float currentRailSpinTime;
    private bool hasRailSpinBoost;
    private float lastRailSpinTime;

    [SerializeField] private float railSpinCooldown = 1.0f;
    [SerializeField] private float railSpinChargeTime = 0.75f;


    [SerializeField] private float railReverseCooldown = 0.25f;
    [SerializeField] public float railReversePauseTime = 0.15f;

    private Coroutine railReverseRoutine;

    private ActionWindow currentActionWindow;

    public Action<ControllerState, ControllerState> stateChanged;
    public Action<AirRail> enterAirRail;
    public Action<AirRail> exitAirRail;
    public Action<bool, bool, int> updateDrift;
    public Action boost;
    public Action superBoost;
    public Action<Transform> updateRaceTarget;
    public Action enterRail;
    public Action exitRail;
    public Action enterWaterfall;
    public Action exitWaterfall;
    public Action<int> dash;
    public Action<string> triggerAnim;
    public Action<string> enableBoolAnim;
    public Action<string> disableBoolAnim;
    public Action railGrindAnim;
    public Action playTargetJumpParticles;
    public Action<bool> togglePlayerBodyVisual;
    public Action straf;
    public Action<float> afterImageEffect;
    public Action<float> superAfterImageEffect;
    public Action updateEquipmentVisual;
    public Action<bool, float> togglePlayerBlinkMat;
    public Action reverseGrinding;
    public Action stomplanding;
    public Action stompJump;
    public Action<ActionWindowType> actionWindowActive;
    public Action spinStart;
    public Action superSpinStart;
    public Action spinCancel;
    public Action<bool> spinCharged;
    public Action style;


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

        //inputs.boost.action.performed += Boost;
        //inputs.drift.action.started += DrifStart;
        //inputs.drift.action.performed += Drift;
        //inputs.drift.action.canceled += DriftReleased;
        inputs.stomp.action.performed += Stomp;
        inputs.jump.action.performed += Jump;
        inputs.jump.action.canceled += Jump;
        //inputs.dash.action.performed += StyleDash;
        inputs.dash.action.performed += CatTimeAbility;
        inputs.dash.action.performed += ReverseRailDirection;
        inputs.strafLeft.action.performed += Straf;
        inputs.strafRight.action.performed += Straf;
        inputs.jump.action.performed += JumpOutOfRail;
        inputs.spin.action.started += Spin;
        inputs.spin.action.canceled += InputSpinRelease;


        //Components Setup
        hoverBehaviour.Initialize(controllerData, rb);
        styleBehaviour.Initialize(controllerData);
        updateEquipmentVisual.Invoke();
    }

    private void OnDisable()
    {
        //inputs.boost.action.performed -= Boost;
        inputs.stomp.action.performed -= Stomp;
        //inputs.drift.action.started -= DrifStart;
        //inputs.drift.action.performed -= Drift;
        //inputs.drift.action.canceled -= DriftReleased;
        inputs.jump.action.performed -= Jump;
        inputs.jump.action.canceled -= Jump;
        //inputs.dash.action.performed -= StyleDash;
        inputs.dash.action.performed -= CatTimeAbility;
        inputs.dash.action.performed -= ReverseRailDirection;
        inputs.strafLeft.action.performed -= Straf;
        inputs.strafRight.action.performed -= Straf;
        inputs.jump.action.performed -= JumpOutOfRail;
        inputs.spin.action.started -= Spin;
        inputs.spin.action.canceled -= InputSpinRelease;
    }

    public void LoadData(GameData data)
    {
        doubleJumpAbility = data.doubleJump;
        chargeBoostAbility = data.chargeBoost;
        stompAbility = data.stomp;
        lavaResistanceAbility = data.lavaResistance;
        alienAntennasAbility = data.alienAntennas;
        grindAbility = data.grind;
        catAbility = data.cat;
    }

    public void SaveData(ref GameData data)
    {
        data.doubleJump = doubleJumpAbility;
        data.chargeBoost = chargeBoostAbility;
        data.stomp = stompAbility;
        data.lavaResistance = lavaResistanceAbility;
        data.alienAntennas = alienAntennasAbility;
        data.grind = grindAbility;
        data.cat = catAbility;
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

    private void Style()
    {
        if(State == ControllerState.RAIL)
        {
            lastDashTimestamp = Time.time;
            consecutiveDashCount = Mathf.Clamp(consecutiveDashCount + 1, 0, controllerData.maxConsecutiveDashCount);
            styleBehaviour.StyleTrigger(hoverBehaviour.normalContainer.position, consecutiveDashCount);
            dash?.Invoke(consecutiveDashCount);
            style?.Invoke();
        }
    }

    public Coroutine catRoutine = null;
    private void CatTimeAbility(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if (State != ControllerState.SURFING)
            return;

        if (IsDrifting)
            return;

        if (CameraTargetDetection.Instance.validNPCTargets.Count != 0)
            return;

        if (catAbility)
        {
            if(catRoutine == null)
            {
                catRoutine = StartCoroutine(UIManager.Instance.gameInterface.CatVideoCoroutine());
            }
        }
    }

    //[HideInInspector] public float jumpChargeTimer { get; private set; }
    //[HideInInspector] public bool chargesJump { get; private set; } = false;
    private void Jump(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if (State == ControllerState.DIVING || State == ControllerState.SWIMMING || State == ControllerState.AIRRIDE)
            return;

        if (actionDelayRoutine != null)
            return;

        if (currentActionWindow != null && currentActionWindow.Type == ActionWindowType.StompLand && currentActionWindow.IsActive)
        {
            currentActionWindow.Success(ActionWindowResult.Jump);
            currentActionWindow = null;
            return;
        }

        if (isCoyote)
        {
            //reset coyote
            currentCoyoteTime = 0;
            //boostBehaviour.IncrementGauge(BoostAction.PerfectJump);
        }

        if(state == ControllerState.LEDGEGRAB)
        {
            ExitLedgeJump();
            return;
        }

        //DEFINE DIRECTION
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * airControl.y + camRight * airControl.x).normalized;


        //RELEASE JUMP
        if (context.performed)
        {
            if (State == ControllerState.SURFING && jumpCount < 1)
            {
                //NORMAL JUMP
                State = ControllerState.JUMPING;
                jumpCount++;

                //rb.linearVelocity = hoverBehaviour.normalContainer.forward * HorizontalVelocity.magnitude;
                rb.linearVelocity = Vector3.ProjectOnPlane(moveDir * HorizontalVelocity.magnitude, NormalContainer.up);
                rb.AddForce((NormalContainer.up * controllerData.upwardImpulseForce /* forceMultiplier*/) + (NormalContainer.forward * controllerData.forwardImpulseForce /* forceMultiplier*/), ForceMode.VelocityChange);
                rb.linearDamping = controllerData.jumpDamping;


                // PLAY FMOD PLAYER ACTION JUMP SOUND
                PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.JUMP);

                //Play anim
                triggerAnim.Invoke("Spin");

                //COMBO
                ComboManager.Instance.AddComboAction(ComboID.Jump);

                if (jumpRoutine != null)
                    StopCoroutine(jumpRoutine);
                jumpRoutine = StartCoroutine(JumpRoutine());
                return;
            }
            else if(State == ControllerState.RAIL && jumpCount < 1)
            {
                //RAIL JUMP
                State = ControllerState.JUMPING;
                jumpCount++;

                var velocity = HorizontalVelocity.magnitude;
                rb.linearVelocity = Vector3.zero;

                Vector3 projForward = Vector3.ProjectOnPlane(NormalContainer.forward, Vector3.up);
                rb.AddForce((Vector3.up * controllerData.upwardImpulseForce /* forceMultiplier*/) + (moveDir * controllerData.railImpulseForce), ForceMode.VelocityChange);
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
                if (doubleJumpAbility)
                {
                    //Default in - air jump
                    if (jumpCount <= 1)
                    {
                        AirDash(controllerData.doubleJumpForwardForce, controllerData.doubleJumpUpForce);
                    }
                }
            }
            else if(State == ControllerState.STOMP )
            {
                if (currentActionWindow != null && currentActionWindow.Type == ActionWindowType.StompBuildup && currentActionWindow.IsActive)
                {
                    if (doubleJumpAbility)
                    {
                        //Default in - air jump
                        if (jumpCount <= 1)
                        {
                            {
                                currentActionWindow.Success(ActionWindowResult.Jump);
                                currentActionWindow = null;
                                return;
                            }
                        }
                    }
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
    private void AirDash(float jumpForwardForce, float jumpUpForce)
    {
        jumpCount = 2;
        State = ControllerState.JUMPING;
        //if (conditions pour target dash true)
        Collider[] colliders = Physics.OverlapSphere(hoverBehaviour.normalContainer.position, controllerData.targetDetectionRadius, controllerData.targetObjectsMask);
        // Check Valid target and choose valid Target

        List<Collider> validColliders = new List<Collider>();
        foreach (Collider target in colliders)
        {
            if (CameraTargetDetection.Instance.validJumpTargets.Contains(target))
            {
                validColliders.Add(target);
            }
        }

        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * airControl.y + camRight * airControl.x).normalized;

        if (isSpinning)
        {
            ActionResetSpin();
        }

        // TARGET JUMP
        if (validColliders.Count > 0)
        {
            int index = 0;
            float distance = 0;
            Transform target = null;
            //Play anim
            triggerAnim.Invoke("TargetJump");
            playTargetJumpParticles.Invoke();
            FOVController.instance.FOVEffect(FOVController.FovEffectType.EXPLOSIF);

            //COMBO
            ComboManager.Instance.AddComboAction(ComboID.TargetJump);


            if (validColliders.Count == 1)
            {
                    target = validColliders[0].transform;
            }
            else if (validColliders.Count > 1)
            {
                distance = Vector3.Distance(validColliders[0].transform.position, hoverBehaviour.normalContainer.position);
                for (int i = 1; i < validColliders.Count; i++)
                {
                    if (CameraTargetDetection.Instance.validJumpTargets.Contains(validColliders[i]))
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

            //Visual
            triggerAnim.Invoke("TargetJump");
            playTargetJumpParticles.Invoke();
            FOVController.instance.FOVEffect(FOVController.FovEffectType.EXPLOSIF);

            //COMBO
            ComboManager.Instance.AddComboAction(ComboID.Jump);

            rb.linearVelocity = moveDir * HorizontalVelocity.magnitude;
            rb.AddForce((NormalContainer.up * jumpUpForce /* forceMultiplier*/) + (NormalContainer.forward * jumpForwardForce /* forceMultiplier*/), ForceMode.VelocityChange);
            rb.linearDamping = controllerData.jumpDamping;

            if (jumpRoutine != null)
                StopCoroutine(jumpRoutine);
            jumpRoutine = StartCoroutine(JumpRoutine());
        }


    }
    private void StompLandJump()
    { 
        State = ControllerState.JUMPING;
        jumpCount++;


        rb.linearVelocity = Vector3.zero;

        rb.AddForce((hoverBehaviour.normalContainer.up * controllerData.upwardImpulseForce * controllerData.stompJumpBonusUpForceMult)
            + hoverBehaviour.normalContainer.forward * controllerData.forwardImpulseForce * controllerData.stompJumpBonusForwardForceMult, 
            ForceMode.VelocityChange);
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.STOMPJUMP);

        //Play anim
        triggerAnim.Invoke("StyleTrigger");
        stompJump?.Invoke();
        boost?.Invoke();

        if (jumpRoutine != null)
            StopCoroutine(jumpRoutine);
        jumpRoutine = StartCoroutine(JumpRoutine());

        //if (isSpinning)
        //{
        //    // Bonus si spinning pendant stomp
        //    Boost(controllerData.spinPerfectBonusForce, transform.forward);
        //}
    }

    private void SetDrift(bool drifting, bool boost = false)
    {
        if (!boost)
        {
            PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.DRIFT);
            PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGINGBOOST);
        }
        else if (boost)
        {
            PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CHARGEDBOOST);
            PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.DRIFT);
        }

        if(forwardDrifting == false)
        {
            this.drifting = drifting;

            if (drifting == false)
            {
                ResetDrift();
            }
        }
        
        int xDir = (int)inputs.airControl.action.ReadValue<Vector2>().x;
        updateDrift.Invoke(drifting, boost, xDir);
    }

    private void DrifStart(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        //if (State == ControllerState.SURFING && IsDrifting)
        //{
        //    PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.DRIFT);
        //}
    }

    private void Drift(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if (State == ControllerState.SURFING)
        {
            if(turn == 0)
            {
                forwardDrifting = true;
                updateDrift?.Invoke(true, false, 0);
                PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CHARGINGBOOST);
            }
            else if (Mathf.Abs(turn) > 0)
            {
                if (CanDrift == false)
                    return;
                PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGINGBOOST);
                driftDir = turn > 0 ? 1 : -1;

                SetDrift(true);
            }
        }
    }

    private void DriftReleased(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (IsLocked)
            return;

        if (IsDrifting && State == ControllerState.SURFING)
        {
            DriftBoost();   

        }
        //if (State == ControllerState.SURFING)
        //{
        //    //Visual Reset
        //    CameraTargetController.instance.ResetCamPos(context);
        //}

        forwardDrifting = false;
        SetDrift(false);
    }

    private void ResetDrift()
    {
        //PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.DRIFT);
        //PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGEDBOOST);
        //PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGINGBOOST);
        currentDriftTime = 0;
        hasDriftBoost = false;
        driftDir = 0;
        //togglePlayerBlinkMat.Invoke(false, 25f);
        forwardDrifting = false;
        updateDrift?.Invoke(false, false, 0);
    }

    private void Boost(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (State == ControllerState.SURFING)
        {
            if(boostRoutine == null && IsDrifting == false)
            {
                boostBehaviour.UseBoost(() => Boost(controllerData.boostForce, Camera.main.transform.forward));
            }
        }

        if (State == ControllerState.JUMPING || State == ControllerState.FALLING)
        {
            if(boostJumpRoutine == null)
            {
                boostBehaviour.UseBoost(() => BoostJump());
            }
        }
    }

    public Coroutine strafRoutine {get; private set; } = null;
    private IEnumerator StrafCooldownRoutine()
    {
        yield return new WaitForSeconds(controllerData.strafCooldown);

        strafRoutine = null;
    }

    private void Straf(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if(grindAbility == true)
        {
            if (strafRoutine == null)
            {
                strafRoutine = StartCoroutine(StrafCooldownRoutine());
                if (State == ControllerState.SURFING)
                {
                        //ENABLE strafHitbox
                        
                        if (context.action.name == InputManager.Instance.strafLeft.action.name)
                        {
                            rb.linearVelocity = Vector3.zero;
                            rb.AddForce(-Camera.main.transform.right * controllerData.strafForce + Camera.main.transform.forward * controllerData.strafForwardForce, ForceMode.VelocityChange);
                            straf.Invoke();
                        }
                        else if (context.action.name == InputManager.Instance.strafRight.action.name)
                        {
                            rb.linearVelocity = Vector3.zero;
                            rb.AddForce(Camera.main.transform.right * controllerData.strafForce + Camera.main.transform.forward * controllerData.strafForwardForce, ForceMode.VelocityChange);
                            straf.Invoke();
                        }
                }
                else if(State == ControllerState.RAIL)
                {
                    if (TryRailTransfer(context))
                        return;

                    StrafOutOfRail(context);
                }
            }
        }
    }

    private Coroutine stompRoutine;
    private void Stomp(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (stompAbility)
        {
            if (state == ControllerState.FALLING || state == ControllerState.JUMPING)
            {
                if (jumpRoutine != null)
                    StopCoroutine(jumpRoutine);

                if (isSpinning)
                {
                    SuperSpin();
                }

                if (stompRoutine == null)
                {
                    stompRoutine = StartCoroutine(StompCoroutine(context));
                }
            }
        }
    }

    private IEnumerator StompCoroutine(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        State = ControllerState.STOMP;
        fallTime = 0f;
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(hoverBehaviour.normalContainer.up * controllerData.stompUpForce, ForceMode.VelocityChange);
        triggerAnim.Invoke("StompCharge");
        fallTime = 0f;
        StartStompBuildup();
        yield return new WaitUntil(() => stompBuildupWindowEnded);
        FOVController.instance.FOVEffect(FOVController.FovEffectType.STOMP);
        triggerAnim.Invoke("Stomp");
        //afterImageEffect.Invoke(controllerData.stompAfterImageEffectTime);
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(-hoverBehaviour.normalContainer.up * controllerData.stompForce, ForceMode.VelocityChange);
        
    }

    private Coroutine boostJumpRoutine;

    private void BoostJump()
    {
        if (state == ControllerState.FALLING || state == ControllerState.JUMPING)
        {
            if(jumpRoutine != null)
                StopCoroutine(jumpRoutine);

            if (boostJumpRoutine == null)
            {
                boostJumpRoutine = StartCoroutine(BoostJumpCoroutine());
            }
        }
    }

    private IEnumerator BoostJumpCoroutine()
    {
        State = ControllerState.BOOSTJUMP;
        rb.linearVelocity = Vector3.zero;
        triggerAnim.Invoke("Boost");
        boost.Invoke();
        afterImageEffect.Invoke(controllerData.boostAfterImageEffectDuration);
        rb.AddForce(Camera.main.transform.forward * controllerData.boostForce, ForceMode.VelocityChange);
        yield return new WaitForSeconds(controllerData.boostCooldown);
        boostJumpRoutine = null;
    }

 

    private void Update()
    {
        //if (Input.GetKeyUp(KeyCode.R))
        //{
        //    Game.Instance.Respawn(out Vector3 position, out Quaternion rotation);
        //}

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

        if(State == ControllerState.FALLING || State == ControllerState.JUMPING || State == ControllerState.AIRRIDE || State == ControllerState.BUMP) 
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



        if (hasResetCam == true)
        {
            CameraTargetController.instance.ResetCamPos(true);
        }
        else if (hasResetCam == false)
        {
            CameraTargetController.instance.ResetCamPos(false);
        }

        if(currentActionWindow != null)
        {
            currentActionWindow.Update();

            if (currentActionWindow.IsFinished)
            {
                currentActionWindow = null;
            }
        }

    }

    bool hasHitWater = false;
    bool hasHitWalls = false;
    bool bumpRail = false;
    bool stompSweetSpot = false;
    float fallTime = 0f;

    //float xRotation = 0f;
    private void FixedUpdate()
    {
        //print(hasResetCam);

        LayerMask waterCheckMask = waterRaycastLayer | obstacleRaycastLayer;

        bool hitSomething = Physics.Raycast(hoverBehaviour.normalContainer.position,-hoverBehaviour.normalContainer.up,out RaycastHit waterInfo,controllerData.hoverRaycastLength,waterCheckMask);


        hasHitWater = hitSomething && ((1 << waterInfo.collider.gameObject.layer) & waterRaycastLayer) != 0;
        hasHitWalls = Physics.Raycast(hoverBehaviour.normalContainer.position, -hoverBehaviour.normalContainer.up, out RaycastHit defaultInfo, controllerData.hoverRaycastLength, defaultRaycastLayer.value);
        bumpRail = Physics.Raycast(hoverBehaviour.normalContainer.position, hoverBehaviour.normalContainer.forward, out RaycastHit railInfo, controllerData.hoverRaycastLength, defaultRaycastLayer.value);
        stompSweetSpot = Physics.Raycast(hoverBehaviour.normalContainer.position, -hoverBehaviour.normalContainer.up, out RaycastHit stompInfo, controllerData.stompCancelRange, defaultRaycastLayer.value);

        if (State != ControllerState.SURFING)
        {
            //Stop drifting
            ResetDrift();
        }

        if (spinBehaviour.spinColEnabled != IsSpinning)
        {
            spinBehaviour.ToggleCollision(IsSpinning);
        }

        if (OnRail)
        {

            State = ControllerState.RAIL;

            if (railLock)
            {
                return;
            }

            if (isRailSpinning)
            {
                currentRailSpinTime += Time.fixedDeltaTime;
                Debug.Log("Current rail spin time= " + currentRailSpinTime);

                if (currentRailSpinTime > railSpinChargeTime && hasRailSpinBoost == false)
                {
                    hasRailSpinBoost = true;

                    spinCharged?.Invoke(true);
                    togglePlayerBlinkMat.Invoke(true, 25f);

                    PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGINGBOOST);
                    PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CHARGEDBOOST);
                }
            }

            if (bumpRail && railInfo.collider.CompareTag("RailCollider"))
            {
                    ReverseRailDirectionNoContext();
                    //currentRail = null;
                    //rb.isKinematic = false;
                    //exitRail.Invoke();
                    //railDetector.ExitRail();
                    //PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.GRINDRAIL);
                    //disableBoolAnim("Grind");
                    //Bump((-hoverBehaviour.normalContainer.forward + hoverBehaviour.normalContainer.up));
            }

            else if (false == currentRail.Progress(Time.fixedDeltaTime, out Vector3 nextPos, out Vector3 normal, out Vector3 direction))
            {
                Rail rail = currentRail;
                OnRailExit(rail);

                currentRail = null;
                rb.isKinematic = false;

                Vector3 exitDir = rail.GetExitDirection();
                rb.AddForce(exitDir * controllerData.railExitForce, ForceMode.VelocityChange);

                exitRail.Invoke();
                railDetector.ExitRail();
                State = ControllerState.SURFING;
                PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.GRINDRAIL);
                disableBoolAnim("Grind");
            }
            else
            {
                transform.position = nextPos + grindOffset;
                NormalContainer.forward = direction;
            }

            return;
        }

        if (OnWaterFall)
        {
            State = ControllerState.SWIMMING;

            if (false == currentWaterFall.FollowSpline(Time.fixedDeltaTime,out Vector3 nextPos,out Vector3 normal,out Vector3 direction))
            {
                currentWaterFall.ToggleWaterFallCamera(false);
                currentWaterFall = null;

                transform.rotation = new Quaternion(0,transform.rotation.y,0,transform.rotation.w);

                rb.isKinematic = false;
                rb.AddForce(direction * 50, ForceMode.VelocityChange);

                exitWaterfall.Invoke();
                railDetector.ExitWaterfall();

                State = ControllerState.SURFING;
                //disableBoolAnim("Grind");
            }
            else
            {
                transform.position = nextPos;
                transform.forward = direction.normalized;
            }

            return;
        }

        if (InAirRail)
        {
            rb.linearVelocity = currentAirRail.direction.forward * currentAirRail.rideForce;
            
            if (currentAirRail.InAirRail(transform.position) == false)
            {
                exitAirRail.Invoke(currentAirRail);
                currentAirRail = null;
            }
            return;
        }

        //DRIFT
        if (IsDrifting)
        {
            currentDriftTime += Time.fixedDeltaTime;
            if (currentDriftTime > controllerData.driftBoostTimer && hasDriftBoost == false)
            {
                PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGINGBOOST);
                hasDriftBoost = true;
                SetDrift(true, true);
                togglePlayerBlinkMat.Invoke(true, 25f);
            }
        }

        if (isSpinning)
        {
            if (!canceledByAction)
            {
                currentSpinTime += Time.fixedDeltaTime;
                if (currentSpinTime > controllerData.spinBoostTimer && hasSpinBoost == false)
                {
                    hasSpinBoost = true;
                    spinCharged?.Invoke(true);
                    togglePlayerBlinkMat.Invoke(true, 25f);
                    PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGINGBOOST);
                    PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CHARGEDBOOST);
                }
            }
        }




        //Falling to Surfing
        if (State == ControllerState.FALLING)
        {
            if (hasHitWater)
            {
                State = ControllerState.SURFING;
                if (stompRoutine != null)
                {
                    stompRoutine = null;
                }
                ResetJump();
                ResetAirSpin();
                fallTime = 0f;
            }

            if (inputs.spin.action.IsPressed())
            {
                Spin(default);
            }
            //else if (hasHitWalls)
            //{
            //    //Vector3 bumpDirection = (hoverBehaviour.normalContainer.position - defaultInfo.point).normalized;
            //    Vector3 bumpDirection = bumptInfo.normal;
            //    Bump(bumpDirection);
            //}
        }

        if(State == ControllerState.FALLING || State == ControllerState.STOMP)
        {
            fallTime += Time.fixedDeltaTime;
        }

        //LEDGEGRAB
        if (State == ControllerState.LEDGEGRAB)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        //Stomping to Surfing
        if (State == ControllerState.STOMP)
        {
            rb.AddForce(-hoverBehaviour.normalContainer.up * controllerData.stompAccelForce, ForceMode.Acceleration);
            if (hasHitWater)
            {
                State = ControllerState.SURFING;

                StartStompLandWindow();

                stompRoutine = null;
                ResetJump();
                fallTime = 0f;
            }
            else if (hasHitWalls)
            {
                State = ControllerState.FALLING;
                rb.AddForce(hoverBehaviour.normalContainer.up * rb.linearVelocity.magnitude / 2f, ForceMode.Acceleration);
            }

            if (currentActionWindow != null && currentActionWindow.Type == ActionWindowType.StompBuildup && currentActionWindow.IsActive && inputs.spin.action.IsPressed() && isSpinning == true)
            {
                Debug.Log("Super spin");
                currentActionWindow.Success(ActionWindowResult.SuperSpin);
                currentActionWindow = null;
                return;
            }
        }

        if(State == ControllerState.BOOSTJUMP)
        {
            if(boostJumpRoutine == null)
            {
                State = ControllerState.FALLING;
            }
        }

        //Jumping / AirRide / Bump to Falling
        if ((State == ControllerState.JUMPING || State == ControllerState.AIRRIDE) && 
            (Vector3.Dot(NormalContainer.up, rb.linearVelocity.normalized) < 0 || hasHitWater) && 
            currentWaterBlock == null && 
            jumpRoutine == null)
        {
            State = ControllerState.FALLING;
        }

        if ((State == ControllerState.BUMP) &&
        (Vector3.Dot(NormalContainer.up, rb.linearVelocity.normalized) < 0) &&
        currentWaterBlock == null &&
        jumpRoutine == null)
        {
            State = ControllerState.FALLING;
        }

        //Jumping to AirRide
        if (State == ControllerState.JUMPING)
        {
            //if(Velocity.y > controllerData.airRideVelocityThreshold)
            //{
            //    State = ControllerState.AIRRIDE;
            //    rb.linearDamping = defaultDrag;
            //}
        }

        //Apply gravity
        if (State == ControllerState.JUMPING ||
            State == ControllerState.AIRRIDE || 
            State == ControllerState.BOOSTJUMP ||
            State == ControllerState.BUMP)
        {
            float force = controllerData.gravity;
            if (State == ControllerState.AIRRIDE)
                force *= controllerData.airRideGravityScale;

            rb.AddForce(Vector3.down * force, ForceMode.Acceleration);
            rb.linearVelocity = ClampYVelocity(Velocity, -controllerData.maxFallingSpeed, float.MaxValue);
        }
        else if(State == ControllerState.FALLING)
        {
            float force = 1f;
            if (fallTime > controllerData.maxAirTime)
            {
                force = controllerData.gravity * controllerData.maxAirTimeGravityFactor;
                rb.AddForce(Vector3.down * force, ForceMode.Acceleration);
                rb.linearVelocity = ClampYVelocity(Velocity, -controllerData.maxFallingSpeed * controllerData.limitFallingSpeedFactor, float.MaxValue);
            }
            else
            {
                force = controllerData.gravity;
                rb.AddForce(Vector3.down * force, ForceMode.Acceleration);
                rb.linearVelocity = ClampYVelocity(Velocity, -controllerData.maxFallingSpeed, float.MaxValue);
            }
        }
        else if (State == ControllerState.STOMP)
        {
            float force = 1f;
            if (fallTime > controllerData.stompChargeTime)
            {
                force = controllerData.gravity * controllerData.maxAirTimeGravityFactor;
                rb.AddForce(Vector3.down * force, ForceMode.Acceleration);
                rb.linearVelocity = ClampYVelocity(Velocity, -controllerData.maxFallingSpeed * controllerData.limitFallingSpeedFactor, float.MaxValue);
            }
        }

        //reset dash counter if over threshold
        if ((Time.time - lastDashTimestamp) > controllerData.styleCooldown + controllerData.dashTimeThreshold)
            consecutiveDashCount = 0;

        //Hover on water
        if (State == ControllerState.SURFING)
        {
            if (hasHitWater)
            {
                hoverBehaviour.Hover(waterInfo, Time.fixedDeltaTime);

                if (HorizontalVelocity.sqrMagnitude > controllerData.minSpeedToDash)
                    currentDashTime += Time.deltaTime;
                else
                    currentDashTime = 0f;
            }
            else
            {
                currentCoyoteTime += Time.fixedDeltaTime;
                if (currentCoyoteTime > controllerData.coyoteTime)
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

        if(State == ControllerState.FALLING || State == ControllerState.DIVING || State == ControllerState.JUMPING)
        {
            AirControl();
        }

        if (State == ControllerState.FALLING || State == ControllerState.JUMPING)
        {
            if (hasHitWater)
                return;

            Vector2 direction = this.airControl.normalized;
            Vector3 airControl = transform.TransformDirection(new Vector3(direction.x, 0, direction.y));

            if (hoverBehaviour.CanLockToSurface(airControl, out Vector3 surfaceNormal, out Vector3 hitPoint))
            {
                NormalContainer.up = surfaceNormal;
                NormalContainer.Rotate(0, transform.eulerAngles.y, 0);
                State = ControllerState.SURFING;
                ResetJump();
            }
        }

        if(State == ControllerState.FALLING && rb.linearVelocity.y < 0 || State == ControllerState.JUMPING){
            if (TryDetectLedge(out Vector3 point, out Vector3 normal))
            {
                EnterLedgeGrab(point, normal);
                return;
            }
        }
    }

    private void AirControl()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.Normalize();
        camRight.Normalize();


        Vector3 direction = (camForward * airControl.y + camRight * airControl.x).normalized;
        direction = Vector3.ProjectOnPlane(direction, Vector3.up);
        float coeff = controllerData.fallingAirControl;

        //if(State == ControllerState.FALLING && turn != 0)
        //    rb.AddTorque(new Vector3(0,Mathf.Sign(turn) * controllerData.airControlRotationSpeed ,0), ForceMode.Acceleration);


        //ROTATION CONTROL
        if (!isSpinning)
        {
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                Quaternion deltaRot = targetRot * Quaternion.Inverse(rb.rotation);

                deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
                if (angle > 180f) angle -= 360f;


                Vector3 torque = axis * angle * Mathf.Deg2Rad * controllerData.airControlRotationSpeed;
                rb.AddTorque(torque, ForceMode.Acceleration);
            }
        }

        var dot = Vector3.Dot(direction, HorizontalVelocity);
        if (dot > 0 && dot > controllerData.maxAirControl)
        {
            rb.linearDamping = 1;
            return;
        }

        rb.linearDamping = 0;
        rb.AddForce(direction * coeff * Time.fixedDeltaTime, ForceMode.VelocityChange);

        //hard clamp -  probably there is a better way to do this eg. add inverse force
        //ClampHorizontalVelocity(controllerData.maxAirControlSpeed);
    }

    public Vector3 direction;
    private void Movement()
    {
        float speed = controllerData.acceleration;

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.Normalize();
        camRight.Normalize();

        float right = inputs.driftR.action.ReadValue<float>();
        float left = inputs.driftL.action.ReadValue<float>();

        turn = 0f;

        bool rightPressed = right > 0.1f;
        bool leftPressed = left > 0.1f;

        bool bothPressed = rightPressed && leftPressed;
        bool onlyOnePressed = rightPressed ^ leftPressed;

        //Handle drift change -> if we are forward drifting and we start turning with enough speed
        if(forwardDrifting && onlyOnePressed && CanDrift)
        {
            forwardDrifting = false;
            driftDir = rightPressed ? 1 : -1;
            SetDrift(true);
            //TODO Coco - here maybe you want to trigger a specific visual or gameplay rule.
        }
        else if(drifting && bothPressed && CanDrift)
        {
            drifting = false;
            forwardDrifting = true;
            driftDir = 0;
            SetDrift(true);
        }

        //Get input and project them in camera reference + project them on avatar plane
        inputDirection = inputs.moveDirection.action.ReadValue<Vector2>();

        if(drifting)
        {
            //0 peut etre mis dans la data pour modifier la valeur de l'input Min / Max dependament de la dir
            float minSteer = driftDir == 1 ? controllerData.steeringRemapMin : -controllerData.steeringRemapMax;
            float maxSteer = driftDir == 1 ? controllerData.steeringRemapMax : -controllerData.steeringRemapMin;
            float remappedTurn = math.remap(-1, 1, minSteer, maxSteer, turn);

            inputDirection = new Vector2(remappedTurn, 0);
        }

        direction = (camForward * inputDirection.y + camRight * inputDirection.x).normalized;

        //If Locked because of ActionWindow => Can't move anymore / Set rotation
        if (actionWindowLocked) return;

        Vector3 targetDir = Vector3.ProjectOnPlane(direction, hoverBehaviour.normalContainer.up);
        Debug.DrawRay(transform.position, targetDir * 5, Color.red); 

        if (targetDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(targetDir);
            Quaternion deltaRot = targetRot * Quaternion.Inverse(rb.rotation);

            deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;

            Vector3 torque = axis * angle * Mathf.Deg2Rad * (IsDrifting? controllerData.driftTurnSpeed : controllerData.rotationTorque);
            rb.AddTorque(torque, ForceMode.Acceleration);
        }

        //final force application
        rb.AddForce(targetDir.normalized * speed, ForceMode.Acceleration);
        if (drifting)
        {
            rb.AddForce(hoverBehaviour.normalContainer.right * driftDir * controllerData.lateralSpeed, ForceMode.Acceleration);
        }


        //Apply drag if braking / forward drifting / drifting
        if (drifting)
        {
            rb.linearDamping = Mathf.Lerp(defaultDrag, controllerData.driftDrag, Time.deltaTime);
        }
        else if (forwardDrifting)
        {
            rb.linearDamping = Mathf.Lerp(defaultDrag, controllerData.forwardDriftDrag, Time.deltaTime);
        }
        else if (brake > 0.0f)
        {
            rb.linearDamping = Mathf.Lerp(defaultDrag, controllerData.brakeForce, brake);
        }
        else
        {
            rb.linearDamping = defaultDrag;
        }

        //hard clamp -  probably there is a better way to do this eg. add inverse force
        //ClampHorizontalVelocity(controllerData.maxSpeed);
    }

    private void ClampHorizontalVelocity(float maxHorizontalMagnitude)
    {
        Vector3 clamped = Vector3.ClampMagnitude(HorizontalVelocity, maxHorizontalMagnitude);
        rb.linearVelocity = new Vector3(clamped.x, rb.linearVelocity.y, clamped.z);
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

    public Coroutine boostRoutine;
    private IEnumerator BoostCoroutine()
    {
        spinBehaviour.ToggleBoostCollision(true);
        yield return new WaitForSeconds(controllerData.boostCooldown);
        spinBehaviour.ToggleBoostCollision(false);
        boostRoutine = null;
    }

    private void Boost(float force, Vector3 direction)
    {
        if(boostRoutine == null)
        {
            boostRoutine = StartCoroutine(BoostCoroutine());
        }
        //rb.AddForce(hoverBehaviour.normalContainer.forward * force, ForceMode.VelocityChange);
        rb.AddForce(direction * force, ForceMode.VelocityChange);
        afterImageEffect.Invoke(controllerData.boostAfterImageEffectDuration);
        boost.Invoke();
        //togglePlayerBlinkMat.Invoke(false,0f);
        FOVController.instance.FOVEffect(FOVController.FovEffectType.BOOST);
        triggerAnim.Invoke("Boost");
    }

    private void SuperBoost(float force, Vector3 direction)
    {
        if (boostRoutine == null)
        {
            boostRoutine = StartCoroutine(BoostCoroutine());
        }
        rb.AddForce(direction * force, ForceMode.VelocityChange);
        superAfterImageEffect?.Invoke(controllerData.superBoostAfterImageEffectDuration);
        superBoost?.Invoke();
        FOVController.instance.FOVEffect(FOVController.FovEffectType.SUPERBOOST);
        triggerAnim.Invoke("Boost");
    }

    private void DriftBoost()
    {
        if (currentDriftTime > controllerData.driftBoostTimer)
        {
            if(boostRoutine == null)
            {
                Boost(controllerData.boostForce, Camera.main.transform.forward);
                boostBehaviour.IncrementGauge(BoostAction.BoostedDrift);
            }
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
        rb.AddForce((hoverBehaviour.normalContainer.forward * propulsionForce / 2f) + (hoverBehaviour.normalContainer.up * propulsionForce / 1.5f), ForceMode.VelocityChange);
        ResetJump();
    }
    public bool CanEnterRail()
    {
        return Time.time - lastRailExitTime > railReenterCooldown;
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
        if (State == ControllerState.RAIL)
            return false;

        if (rail == lastRail && Time.time - lastRailExitTime < railReenterCooldown)
            return false;

        if (grindAbility)
        {
            ComboManager.Instance.AddComboAction(ComboID.RailEnter);
            ComboManager.Instance.SetComboTimerFrozen(true);
            ResetJump();
            ActionResetSpin();
            CancelActionWindow();
            railReverseRoutine = null;


            currentRail = rail;

            ResetSpin();

            Vector3 intentDir = GetRailIntentDirection();
            rail.EnterRail(transform.position, intentDir);

            rb.isKinematic = true;
            boost.Invoke();
            enterRail.Invoke();
            enableBoolAnim.Invoke("Grind");
            triggerAnim("StartGrind");

            PlayerActionFMODManager.Instance.PlayPlayerActionWithParam(
                PlayerActionFMOD.GRINDRAIL,
                "L_Grind_Surface",
                (float)rail.railType
            );

            CancelStomp();

            return true;
        }
        return false;
    }

    private Vector3 GetRailIntentDirection()
    {
        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        // Input prioritaire
        Vector2 move = inputs.moveDirection.action.ReadValue<Vector2>();

        Vector3 intent =
            camForward * move.y +
            camRight * move.x;

        // Si pas d'input → fallback velocity
        if (intent.sqrMagnitude < 0.01f)
            intent = HorizontalVelocity;

        // Si toujours rien → fallback caméra
        if (intent.sqrMagnitude < 0.01f)
            intent = camForward;

        intent.y = 0f;
        return intent.normalized;
    }

    private void ReverseRailDirection(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (State != ControllerState.RAIL)
            return;

        if (currentRail == null)
            return;

        if (railReverseRoutine != null)
            return;

        railReverseRoutine = StartCoroutine(ReverseRailRoutine());
    }

    private void ReverseRailDirectionNoContext()
    {
        railReverseRoutine = StartCoroutine(ReverseRailRoutine());
    }

    private bool isReverseRouting = false;
    private IEnumerator ReverseRailRoutine()
    {
        RailLock(true);

        //RESET LE SPIN
        ResetRailSpin();

        //ANIMATION
        reverseGrinding?.Invoke();

        isReverseRouting = true;
        yield return new WaitForSeconds(railReversePauseTime);

        isReverseRouting = false;
        currentRail.Reverse();

        NormalContainer.forward = -NormalContainer.forward;

        RailLock(false);

        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.GRINDRAIL);

        yield return new WaitForSeconds(railReverseCooldown);
        railReverseRoutine = null;
    }

    public bool EnterWaterfall(WaterFall waterfall)
    {
        if(State == ControllerState.SWIMMING)
            return false;

        CancelActionWindow();

        currentWaterFall = waterfall;
        waterfall.EnterWaterFall();
        enterWaterfall.Invoke();
        rb.isKinematic = true;
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

        if (State == ControllerState.FALLING || State == ControllerState.JUMPING || State == ControllerState.AIRRIDE || State == ControllerState.BUMP)
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
        ResetJump();
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

        if (lockController)
            CancelActionWindow();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = lockController;
    }

    public void RailLock(bool lockValue)
    {
        railLock = lockValue;
    }

    private void ResetJump() => jumpCount = 0;

    public void UpdateRaceTarget(Transform target) => updateRaceTarget.Invoke(target);

    public void UnlockAbility(string ability)
    {
        if(Enum.TryParse(ability,true,out ControllerAbility parsedAbility))
        switch (parsedAbility)
        {
                case ControllerAbility.DOUBLEJUMP:
                    doubleJumpAbility = true;
                    break;
                case ControllerAbility.STOMP:
                    stompAbility = true;
                    break;
                case ControllerAbility.CHARGEBOOST:
                    chargeBoostAbility = true;
                    break;
                case ControllerAbility.LAVARESIST:
                    lavaResistanceAbility = true;
                    break;
                case ControllerAbility.ALIEN:
                    alienAntennasAbility = true;
                    break;
                case ControllerAbility.GRIND:
                    grindAbility = true;
                    break;
                case ControllerAbility.CAT:
                    catAbility = true;
                    break;
                default:
                    break;
        }
        updateEquipmentVisual.Invoke();
    }

    public void ToggleDialogState(bool value)
    {
        if (value)
        {
            state = ControllerState.DIALOG;
            ForceLock(true);
        }
        else
        {
            state = ControllerState.SURFING;
            ForceLock(false);
        }

    }

    private Coroutine bumpRoutine;
    public void Bump(Vector3 direction)
    {
        if (state == ControllerState.BUMP)
            return;

        if (bumpRoutine != null)
            return;

        bumpRoutine = StartCoroutine(BumpCoroutine());
        state = ControllerState.BUMP;
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.BUMP);
        Vector3 force = (NormalContainer.up * controllerData.bumpForce) + (direction * controllerData.forwardImpulseForce);
        rb.AddForce(force, ForceMode.VelocityChange);
    }

    private IEnumerator BumpCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        bumpRoutine = null;
    }

    public void JumpOutOfRail(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (currentRail != null && State == ControllerState.RAIL && !isReverseRouting)
        {
            OnRailExit(currentRail);

            currentRail = null;
            hoverBehaviour.normalContainer.up = Vector3.up;

            rb.isKinematic = false;
            exitRail.Invoke();
            railDetector.ExitRail();
            disableBoolAnim("Grind");
            PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.GRINDRAIL );
            ResetRailSpin();
            Jump(context);
            ComboManager.Instance.SetComboTimerFrozen(false);
        }
    }

    public void OnRailExit(Rail rail)
    {
        ComboManager.Instance.SetComboTimerFrozen(false);
        lastRail = rail;
        lastRailExitTime = Time.time;
    }

    public void StrafOutOfRail(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (currentRail != null && State == ControllerState.RAIL && !isReverseRouting)
        {
            ResetRailSpin();
            OnRailExit(currentRail);

            currentRail = null;
            rb.isKinematic = false;

            exitRail.Invoke();
            PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.GRINDRAIL);
            railDetector.ExitRail();
            disableBoolAnim("Grind");

            State = ControllerState.SURFING;
            ComboManager.Instance.SetComboTimerFrozen(false);

            if (disableCollisionRoutine == null)
            {
                disableCollisionRoutine = StartCoroutine(DisableCollisionTemporarily(0.15f));
            }

            if (context.action.name == InputManager.Instance.strafLeft.action.name)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(
                    -NormalContainer.right * controllerData.strafForce +
                     NormalContainer.forward * controllerData.strafForwardForce,
                    ForceMode.VelocityChange
                );
                straf.Invoke();
            }
            else if (context.action.name == InputManager.Instance.strafRight.action.name)
            {
                rb.linearVelocity = Vector3.zero;
                rb.AddForce(
                    NormalContainer.right * controllerData.strafForce +
                    NormalContainer.forward * controllerData.strafForwardForce,
                    ForceMode.VelocityChange
                );
                straf.Invoke();
            }
        }
    }

    private void CancelStomp()
    {
        //Reset du Stomp
        if(stompRoutine != null)
        {
            StopCoroutine(stompRoutine);
            stompRoutine = null;
        }
    }

    private Coroutine actionDelayRoutine;

    private IEnumerator ActionDelay()
    {
        yield return new WaitForSeconds(0.15f);
        actionDelayRoutine = null;
    }

    public void Spin(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        //if(state == ControllerState.STOMP && (currentActionWindow == null || currentActionWindow.Type != ActionWindowType.StompBuildup))
        //{

        //    return;
        //}

        if(state == ControllerState.SURFING)
        {
            if(isSpinning == false)
            {
                actionDelayRoutine = StartCoroutine(ActionDelay());
                isSpinning = true;
                spinStart?.Invoke();
                PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CHARGINGBOOST);
            }
        }
        else if(state == ControllerState.FALLING)
        {
            if(isSpinning == false && hasAirSpin == false)
            {
                actionDelayRoutine = StartCoroutine(ActionDelay());

                hasAirSpin = true;
                isSpinning = true;
                spinStart?.Invoke();
                PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CHARGINGBOOST);

                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
                Vector3 force = (NormalContainer.up * controllerData.bumpForce) + (direction * controllerData.forwardImpulseForce);       
                rb.AddForce(force, ForceMode.VelocityChange);
            }
        }

        else if (state == ControllerState.STOMP)
        {
            if (currentActionWindow != null && currentActionWindow.Type == ActionWindowType.StompBuildup && currentActionWindow.IsActive && inputs.spin.action.IsPressed() && isSpinning == false)
            {
                currentActionWindow.Success(ActionWindowResult.Spin);
                currentActionWindow = null;
                return;
            }
        }

        else if (state == ControllerState.RAIL)
        {
            StartRailSpin();
        }
    }

    public void SuperSpin()
    {
        isSuperSpinning = true;
        isSpinning = false;
        superSpinStart?.Invoke();
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CHARGINGBOOST);
    }

    private bool canceledByAction;
    public void InputSpinRelease(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (state == ControllerState.STOMP && isSuperSpinning)
            return;

        SpinRelease();
    }

    public void SpinRelease()
    {
        if (state == ControllerState.STOMP && isSuperSpinning)
            return;

        if (canceledByAction == false)
        {
            if (state == ControllerState.SURFING)
            {
                if (hasSpinBoost == true)
                {
                    //COMBO
                    ComboManager.Instance.AddComboAction(ComboID.SpinBoost);

                    Boost(controllerData.boostForce, Camera.main.transform.forward);
                }


            }
            else if (state == ControllerState.FALLING || state == ControllerState.JUMPING)
            {
                if (hasSpinBoost == true)
                {
                    //COMBO
                    ComboManager.Instance.AddComboAction(ComboID.SpinAirBoost);

                    Boost(controllerData.boostForce, Camera.main.transform.forward);
                }
            }

            else if (state == ControllerState.RAIL)
            {
                ReleaseRailSpin();
                return;
            }
        }
        ResetSpin();
    }

    public void SuperSpinBoost()
    {
        Boost(controllerData.superBoostForce, Camera.main.transform.forward);
    }
    private void ActionResetSpin()
    {
        canceledByAction = true;
        ResetSpin();
    }

    private void ResetSpin()
    {
        spinCancel?.Invoke();

        canceledByAction = false;
        currentSpinTime = 0;
        hasSpinBoost = false;

        isSpinning = false;

        if (!isRailSpinning)
        {
            spinCharged?.Invoke(false);
            togglePlayerBlinkMat.Invoke(false, 25f);
        }

        PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGEDBOOST);
        PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGINGBOOST);

        isSuperSpinning = false;
    }

    private void ResetAirSpin()
    {
        hasAirSpin = false;
    }

    public Coroutine spinBounceRoutine;

    public IEnumerator SpinBounceCoroutine()
    {
        yield return new WaitForSeconds(controllerData.spinBounceTimer);
        spinBounceRoutine = null;
    }

    public void SpinBounce(Vector3 hitNormal)
    {
        if (spinBounceRoutine != null)
            return;

        spinBounceRoutine = StartCoroutine(SpinBounceCoroutine());

        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.BUMP);

        rb.angularVelocity = Vector3.zero;

        Vector3 incomingVelocity = rb.linearVelocity;

        float spinBaseForce = controllerData.spinForce;

        Vector3 spinImpulse = Vector3.ProjectOnPlane(transform.forward, hitNormal).normalized * spinBaseForce;

        Vector3 combinedVelocity = incomingVelocity + spinImpulse;

        Vector3 reflectedVelocity = Vector3.Reflect(combinedVelocity, hitNormal);
        reflectedVelocity += NormalContainer.up * (spinBaseForce * 0.2f);

        float restitution = 0.9f;
        reflectedVelocity *= restitution;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(reflectedVelocity, ForceMode.VelocityChange);
    }

    private bool TryDetectLedge(out Vector3 ledgePoint, out Vector3 ledgeNormal)
    {
        ledgePoint = Vector3.zero;
        ledgeNormal = Vector3.zero;

        if (Time.time - lastLedgeGrabTime < ledgeCooldown)
            return false;

        Vector3 chest = transform.position + Vector3.up * 1.2f;

        // chercher mur devant le joueur
        if (!Physics.SphereCast(
            chest,
            ledgeWallProbeRadius,
            transform.forward,
            out RaycastHit wallHit,
            ledgeWallProbeDistance,
            wallLayer))
            return false;

        // point au dessus du mur
        Vector3 topOrigin = wallHit.point + Vector3.up * ledgeHeight;

        if (!Physics.Raycast(
            topOrigin,
            Vector3.down,
            out RaycastHit topHit,
            ledgeHeight + 0.75f,
            waterLayer))
            return false;

        float heightDifference = topHit.point.y - transform.position.y;

        if (heightDifference < 0.5f || heightDifference > ledgeHeight + 0.4f)
            return false;

        ledgePoint = topHit.point;
        ledgeNormal = wallHit.normal;

        return true;
    }

    private void StartRailSpin()
    {
        if (isRailSpinning)
            return;

        if (Time.time - lastRailSpinTime < railSpinCooldown)
            return;

        isRailSpinning = true;
        currentRailSpinTime = 0f;
        hasRailSpinBoost = false;

        spinStart?.Invoke();
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CHARGINGBOOST);
    }

    private void ReleaseRailSpin()
    {
        if (!isRailSpinning)
            return;

        if (hasRailSpinBoost)
        {
            Style();
        }

        ResetRailSpin();
    }
    private void ResetRailSpin()
    {
        isRailSpinning = false;
        hasRailSpinBoost = false;
        currentRailSpinTime = 0;

        lastRailSpinTime = Time.time;

        spinCancel?.Invoke();
        spinCharged?.Invoke(false);

        togglePlayerBlinkMat.Invoke(false, 25f);

        PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGEDBOOST);
        PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.CHARGINGBOOST);
    }

    private void EnterLedgeGrab(Vector3 point, Vector3 normal)
    {
        State = ControllerState.LEDGEGRAB;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // Cache orientation AVANT grab (utile si tu veux restaurer Up/Forward)
        savedPreLedgeUp = NormalContainer.up;
        savedPreLedgeForward = NormalContainer.forward;

        ledgePoint = point;
        ledgeNormal = normal.normalized;

        // Forward "propre" pour la sortie : on enlève la composante verticale
        savedLedgeExitForward = Vector3.ProjectOnPlane(-ledgeNormal, Vector3.up).normalized;
        if (savedLedgeExitForward.sqrMagnitude < 0.0001f)
            savedLedgeExitForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        Vector3 hangPos = point - ledgeNormal * ledgeForwardOffset;
        hangPos.y -= 0.5f;

        transform.position = hangPos;

        transform.forward = -ledgeNormal;

        lastLedgeGrabTime = Time.time;

        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.BUMP);

        triggerAnim?.Invoke("LedgeGrab");
    }

    private void ExitLedgeJump()
    {
        rb.isKinematic = false;

        NormalContainer.up = Vector3.up;

        // Applique la direction de sortie basée sur la normal capturée
        Vector3 exitForward = savedLedgeExitForward;
        transform.forward = exitForward;

        State = ControllerState.JUMPING;

        rb.AddForce(
            Vector3.up * controllerData.upwardImpulseForce +
            exitForward * controllerData.forwardImpulseForce,
            ForceMode.VelocityChange
        );

        triggerAnim.Invoke("Spin");

        lastLedgeGrabTime = Time.time;
    }
    private void ClimbLedge()
    {
        rb.isKinematic = false;

        Vector3 climbPos = ledgePoint + Vector3.up * 1.2f;
        transform.position = climbPos;

        // reset orientation propre
        NormalContainer.up = Vector3.up;
        transform.forward = savedLedgeExitForward;

        State = ControllerState.SURFING;
    }

    private void DropLedge()
    {
        rb.isKinematic = false;
        State = ControllerState.FALLING;

        // reset orientation propre
        NormalContainer.up = Vector3.up;
        transform.forward = savedLedgeExitForward;
    }

    private void TransferSpinToRail()
    {
        // stop ground spin state
        isSpinning = false;

        // start rail spin
        isRailSpinning = true;

        if (hasRailSpinBoost)
        {
            spinCharged?.Invoke(true);
            togglePlayerBlinkMat.Invoke(true, 25f);
        }

        currentRailSpinTime = currentSpinTime;
        hasRailSpinBoost = hasSpinBoost;

        currentSpinTime = 0;
        hasSpinBoost = false;

        spinStart?.Invoke();
    }

    private bool TryRailTransfer(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.action.name == InputManager.Instance.strafLeft.action.name)
        {
            Rail rail = railDetector.leftRailCandidate;

            if (rail != null)
            {
                railDetector.ConfirmTransfer(false);

                StartCoroutine(RailTransfer(rail, railDetector.LeftHitPoint));
                return true;
            }
        }

        if (context.action.name == InputManager.Instance.strafRight.action.name)
        {
            Rail rail = railDetector.rightRailCandidate;

            if (rail != null)
            {
                railDetector.ConfirmTransfer(true);

                StartCoroutine(RailTransfer(rail, railDetector.RightHitPoint));
                return true;
            }
        }

        return false;
    }
    private IEnumerator RailTransfer(Rail targetRail, Vector3 targetPoint)
    {
        ResetRailSpin();

        OnRailExit(currentRail);

        currentRail = null;
        rb.isKinematic = false;

        exitRail?.Invoke();
        railDetector.ExitRail();

        disableBoolAnim("Grind");
        PlayerActionFMODManager.Instance.TryStopLoopingSound(PlayerActionFMOD.GRINDRAIL);

        State = ControllerState.SURFING;

        rb.linearVelocity = Vector3.zero;

        Vector3 toTarget = targetPoint - transform.position;
        toTarget.y = 0f;

        Vector3 lateralDir = toTarget.normalized;

        Vector3 force =
            lateralDir * controllerData.strafForce +
            NormalContainer.forward * controllerData.strafForwardForce;

        rb.AddForce(force, ForceMode.VelocityChange);

        straf?.Invoke();

        StartCoroutine(DisableCollisionTemporarily(0.15f));

        yield return new WaitForSeconds(0.25f);

        railDetector.ResetTransferPreview();
        EnterRail(targetRail);
    }

    private Coroutine disableCollisionRoutine;

    private IEnumerator DisableCollisionTemporarily(float duration)
    {

        Collider col = GetComponent<Collider>();

        if (col != null)
            col.enabled = false;

        yield return new WaitForSeconds(duration);

        if (col != null)
            col.enabled = true;

        disableCollisionRoutine = null;
    }

    #region ActionWindows
    private void StartActionWindow(ActionWindow window, float duration)
    {
        CancelActionWindow();

        currentActionWindow = window;
        currentActionWindow.Start(duration);
    }

    private void CancelActionWindow()
    {
        if(currentActionWindow != null)
        {
            currentActionWindow.OnFail?.Invoke();
            currentActionWindow.OnEnd?.Invoke();
            currentActionWindow = null;
        }
    }

    #region StompLandWindow

    private void StartStompLandWindow()
    {
        StartActionWindow(CreateStompLandWindow(), controllerData.stompActionWindowTime);
    }

    private bool superSpinOnLand;
    private ActionWindow CreateStompLandWindow()
    {
        ActionWindow stompWindow = new ActionWindow();
        stompWindow.Type = ActionWindowType.StompLand;

        stompWindow.OnStart = () =>
        {

            //VFX dans manta visual
            actionWindowActive?.Invoke(stompWindow.Type);

            PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.BUMP);

            actionWindowLocked = true;

            superSpinOnLand = isSuperSpinning;

            FOVController.instance.FOVEffect(FOVController.FovEffectType.STOMPLAND);

            MantaVisuals.instance.AlignToCamForward();

            if (isSuperSpinning)
            {
                ResetSpin();
            }
        };

        stompWindow.OnSuccess = (result) =>
        {
            switch (result)
            {
                case ActionWindowResult.Jump:
                    ComboManager.Instance.AddComboAction(ComboID.TornadoJump);
                    StompLandJump();
                    break;
            }

        };

        stompWindow.OnFail = () =>
        {
            if (superSpinOnLand)
            {
                ComboManager.Instance.AddComboAction(ComboID.GalaxyBoost);
                if (direction.magnitude > 0.1f)
                {
                    SuperBoost(controllerData.superBoostForce, direction);
                }
                else
                {
                    SuperBoost(controllerData.superBoostForce, NormalContainer.forward);
                }
            }
            else
            {
                ComboManager.Instance.AddComboAction(ComboID.DiveBoost);
                if (direction.magnitude > 0.1f)
                {
                    Boost(controllerData.boostForce, direction);
                }
                else
                {
                    Boost(controllerData.boostForce, NormalContainer.forward);
                }
            }
        };

        stompWindow.OnCancel = () =>
        {
            Debug.Log("Stomp Window has been canceled");
        };

        stompWindow.OnEnd = () =>
        {
            actionWindowLocked = false;
        };

        return stompWindow;
    }
    #endregion

    #region StompBuildup


    private void StartStompBuildup()
    {
        StartActionWindow(CreateStompBuildupWindow(), controllerData.stompActionBuildupWindowTime);
    }



    private ActionWindow CreateStompBuildupWindow()
    {
        ActionWindow stompWindow = new ActionWindow();
        stompWindow.Type = ActionWindowType.StompBuildup;

        stompWindow.OnStart = () =>
        {

            //VFX dans manta visual
            actionWindowActive?.Invoke(stompWindow.Type);

            actionWindowLocked = true;
            stompBuildupWindowEnded = false;

            if (spinBufferedFromStompBuildup)
            {
                spinBufferedFromStompBuildup = false;
                stompWindow.Success(ActionWindowResult.Spin);
                return;
            }
        };

        stompWindow.OnSuccess = (result) =>
        {

            CancelStomp();

            switch (result)
            {
                case ActionWindowResult.Jump:
                    ActionResetSpin();
                    if (jumpCount <= 1)
                    {
                        AirDash(controllerData.stompJumpCancelForwardForce, controllerData.stompJumpCancelUpForce);
                    }
                    break;
                case ActionWindowResult.Spin:
                    if (hasAirSpin == false)
                    {
                        State = ControllerState.FALLING;

                        hasAirSpin = true;
                        isSpinning = true;
                        spinStart?.Invoke();
                        triggerAnim.Invoke("Spin");
                        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.CHARGINGBOOST);

                        rb.linearVelocity = new Vector3(rb.linearVelocity.x / 2, 0, rb.linearVelocity.z / 2);

                        Vector3 force = (NormalContainer.up * controllerData.stompSpinCancelUpForce);

                        rb.AddForce(force, ForceMode.VelocityChange);
                    }
                    break;
                case ActionWindowResult.SuperSpin:
                    SuperSpin();
                    triggerAnim.Invoke("Spin");
                    break;
                
            }
        };

        stompWindow.OnFail = () =>
        {
            if(isSpinning == true)
            {

            }
        };

        stompWindow.OnCancel = () =>
        {

        };

        stompWindow.OnEnd = () =>
        {
            actionWindowLocked = false;
            stompBuildupWindowEnded = true;
        };

        return stompWindow;
    }
    #endregion
    #endregion
}
