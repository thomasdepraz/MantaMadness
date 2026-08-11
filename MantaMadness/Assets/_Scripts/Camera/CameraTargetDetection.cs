using FMODUnity;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraTargetDetection : MonoBehaviour
{
    public static CameraTargetDetection Instance;

    private Transform playerTransform;

    [Header("Jump Target Detection")]
    [SerializeField] private float jumpDetectionRange;
    [SerializeField] private float activationRangeMultiplier = 1.8f;
    [SerializeField] private float approachingRange = 3f;
    [SerializeField] float jumpRangeBuffer =  0.75f;
    [SerializeField] private float jumpCameraAngleBonus = 20f;

    [Header("Target Deadzone")]
    [SerializeField] private float targetInnerDeadzone = 0.35f;
    [SerializeField] private float targetOuterDeadzone = 0.50f;

    [Header("NPC Target Detection")]
    [SerializeField] private float npcDetectionRange;
    [SerializeField] private float npcFixedCamDetectionRange;

    [Header("Shop Target Detection")]
    [SerializeField] private float shopDetectionRange;
    [SerializeField] private LayerMask shopTargetMask;

    [Header("Parameters")]
    [SerializeField] private float viewAngle;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask jumpargetMask;
    [SerializeField] private LayerMask npcTargetMask;
    [SerializeField] private float angleWeight = 1.5f;

    [Header("Target Lists")]
    [SerializeField] public List<Collider> validJumpTargets = new List<Collider>();
    [SerializeField] public List<Collider> validNPCTargets = new List<Collider>();
    [SerializeField] public List<Collider> validShopTargets = new List<Collider>();

    [SerializeField] private EventReference addTargetSound;

    [SerializeField] private List<CinemachineCamera> playerCameras = new List<CinemachineCamera>();

    private CinemachineBrain brain;

    private Collider currentJumpTarget;

    private int targetDashCount = 0;
    [SerializeField] private string dashCountParameterName = "DashCount";

    private SimpleController player;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        if (playerTransform == null && Game.Instance != null && Game.Instance.player != null)
            playerTransform = Game.Instance.player.transform;

        jumpDetectionRange = Game.Instance.player.controllerData.targetDetectionRadius;
        npcDetectionRange = Game.Instance.player.controllerData.npcInteractionRadius;
        npcFixedCamDetectionRange = Game.Instance.player.controllerData.npcFixedCamInteractionRadius;
        shopDetectionRange = Game.Instance.player.controllerData.npcInteractionRadius;

        brain = Camera.main.GetComponent<CinemachineBrain>();
        viewAngle = Camera.main.fieldOfView;
        player = Game.Instance.player;

        if (ComboManager.Instance != null)
            ComboManager.Instance.OnComboEnded += ResetTargetDashCount;
    }
    private void OnDisable()
    {
        if (ComboManager.Instance != null)
            ComboManager.Instance.OnComboEnded -= ResetTargetDashCount;
    }

    private void LateUpdate()
    {
        Physics.SyncTransforms();

        if(PauseMenu.instance != null && !PauseMenu.instance.isPaused)
        {
            DetectJumpTargets();
            DetectNPCTargets();
            DetectShopTargets();
        }
    }

    private float ComputeTargetScore(Vector3 targetPos)
    {
        Vector3 origin = playerTransform.position;
        Vector3 camForward = Camera.main.transform.forward;

        Vector3 toTarget = targetPos - origin;

        float distance = toTarget.magnitude;
        float maxDist = jumpDetectionRange; // ou npc/shop selon usage
        float normalizedDistance = distance / maxDist;

        float angle = Vector3.Angle(camForward, toTarget.normalized);
        float normalizedAngle = angle / (GetCurrentViewAngle() / 2f);

        return normalizedDistance + normalizedAngle * angleWeight;
    }

    private bool IsPlayerCameraActive()
    {
        if (brain == null || brain.ActiveVirtualCamera == null)
            return true; // fallback safe

        var activeCam = brain.ActiveVirtualCamera as CinemachineCamera;
        if (activeCam == null)
            return false;

        return playerCameras.Contains(activeCam);
    }
    public float GetCurrentViewAngle()
    {
        if (Camera.main != null)
            return Camera.main.fieldOfView;

        return viewAngle;
    }

    public Vector3 GetDetectionOrigin()
    {   
        if (CameraZone.ActiveZone != null && playerTransform != null)
            return playerTransform.position;

        if (Camera.main != null)
            return Camera.main.transform.position;

        return transform.position;
    }

    public Vector3 GetDetectionForward()
    {
        if (CameraZone.ActiveZone != null && playerTransform != null)
            return playerTransform.forward;

        if (Camera.main != null)
            return Camera.main.transform.forward;

        return transform.forward;
    }

    private bool IsTargetInsideDeadzone(Vector3 worldPos, bool alreadyTargeted)
    {
        Camera cam = Camera.main;

        Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

        // derrière caméra
        if (viewportPos.z < 0f)
            return false;

        // distance au centre écran
        float dx = viewportPos.x - 0.5f;
        float dy = viewportPos.y - 0.5f;

        float distFromCenter = Mathf.Sqrt(dx * dx + dy * dy);

        // hysteresis :
        // une target déjà lockée a une zone de sortie plus grande
        float limit = alreadyTargeted
            ? targetOuterDeadzone
            : targetInnerDeadzone;

        return distFromCenter < limit;
    }

    void DetectJumpTargets()
    {
        if (player.doubleJumpAbility == false)
            return;

        Collider[] targets = Physics.OverlapSphere(
            playerTransform.position,
            jumpDetectionRange,
            jumpargetMask);

        Collider bestTarget = null;
        float bestScore = float.MaxValue;

        foreach (var col in targets)
        {
            if (!col.TryGetComponent(out JumpTarget jt)) continue;
            if (!jt.isAvailable) continue;

            bool insideDeadzone = IsTargetInsideDeadzone(col.transform.position,col == currentJumpTarget);

            if (!insideDeadzone)
                continue;

            Vector3 dir = (col.transform.position - playerTransform.position).normalized;
            float dist = Vector3.Distance(playerTransform.position, col.transform.position);

            // Vérif obstacle
            if (Physics.Raycast(playerTransform.position, dir, dist, obstacleMask))
                continue;

            float score = ComputeTargetScore(col.transform.position);

            if (col == currentJumpTarget)
            {
                score *= 0.75f;
            }

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = col;
            }
        }

        // --- UPDATE VISUALS ---
        if (currentJumpTarget != null && currentJumpTarget != bestTarget)
        {
            if (currentJumpTarget.TryGetComponent(out JumpTarget oldJT))
                oldJT.SetAsCurrentTarget(false);
        }

        validJumpTargets.Clear();

        if (bestTarget != null)
        {
            if (bestTarget != currentJumpTarget)
                PlayTargetDetectedSound();

            validJumpTargets.Add(bestTarget);

            if (bestTarget.TryGetComponent(out JumpTarget jt))
                jt.SetAsCurrentTarget(true);

            currentJumpTarget = bestTarget;
        }
        else
        {
            // désactive si plus rien
            if (currentJumpTarget != null)
            {
                if (currentJumpTarget.TryGetComponent(out JumpTarget jt))
                    jt.SetAsCurrentTarget(false);
            }

            currentJumpTarget = null;
        }
    }

    void DetectNPCTargets()
    {
        if (player.IsLocked)
            return;

        if (CinematicManager.instance.isCinematicPlaying)
            return;


        if (CameraManager.Instance.isCinematicPlaying)
            return;
 

        if (DialogManager.instance.currentSequence != null)
        {
            ClearNPCTargets();
            return;
        }

        bool useCameraLogic = IsPlayerCameraActive();

        Collider[] targetsInRange;

        if (CameraZone.ActiveZone != null)
        {
            targetsInRange = Physics.OverlapSphere(GetDetectionOrigin(), npcFixedCamDetectionRange, npcTargetMask);
        }
        else
        {
            targetsInRange = Physics.OverlapSphere(GetDetectionOrigin(), npcDetectionRange, npcTargetMask);
        }


        for (int i = validNPCTargets.Count - 1; i >= 0; i--)
        {
            Collider npc = validNPCTargets[i];

            if (npc == null ||
                Vector3.Distance(GetDetectionOrigin(), npc.transform.position) > npcDetectionRange ||
                !npc.TryGetComponent<InteractableNPC>(out _))
            {
                validNPCTargets.RemoveAt(i);
                npc?.GetComponent<InteractableNPC>()?.DisableVisual();
                UIManager.Instance.dialogInteractDisplay.ToggleInterface(false);
            }
        }

        foreach (Collider target in targetsInRange)
        {
            if (!target.TryGetComponent(out InteractableNPC npc))
                continue;

            Vector3 directionToTarget = (target.transform.position - GetDetectionOrigin()).normalized;
            float distanceToTarget = Vector3.Distance(GetDetectionOrigin(), target.transform.position);

            bool isValid = false;

            if (useCameraLogic)
            {
                float currentViewAngle = GetCurrentViewAngle();
                float angleToTarget = Vector3.Angle(GetDetectionForward(), directionToTarget);
                bool inFOV = angleToTarget < currentViewAngle / 2f;
                bool blocked = Physics.Raycast(
                    GetDetectionOrigin(),
                    directionToTarget,
                    distanceToTarget,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore);

                isValid = inFOV && !blocked;
            }
            else
            {
                // fallback : uniquement la distance
                // tu peux ajouter blocked si tu veux
                bool blocked = Physics.Raycast(
                    GetDetectionOrigin(),
                    directionToTarget,
                    distanceToTarget,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore);

                isValid = !blocked; // ou juste true si tu veux vraiment "distance only"
            }

            if (isValid)
            {
                if (!validNPCTargets.Contains(target))
                {
                    validNPCTargets.Add(target);
                    npc.EnableVisual();
                    UIManager.Instance.dialogInteractDisplay.ToggleInterface(true);
                }
            }
            else
            {
                if (validNPCTargets.Contains(target))
                {
                    validNPCTargets.Remove(target);
                    npc.DisableVisual();
                    UIManager.Instance.dialogInteractDisplay.ToggleInterface(false);
                }
            }
        }
    }

    void DetectShopTargets()
    {
        if (validNPCTargets.Count > 0) return;

        bool useCameraLogic = IsPlayerCameraActive();

        Collider[] targetsInRange = Physics.OverlapSphere(
            GetDetectionOrigin(),
            shopDetectionRange,
            shopTargetMask);

        ShopStand bestShop = null;
        Collider bestCollider = null;

        float bestScore = float.MaxValue;

        foreach (Collider target in targetsInRange)
        {
            if (!target.TryGetComponent(out ShopStand shop)) continue;
            if (!shop.IsActive) continue;

            Vector3 directionToTarget = (target.transform.position - GetDetectionOrigin()).normalized;
            float distanceToTarget = Vector3.Distance(GetDetectionOrigin(), target.transform.position);

            bool blocked = Physics.Raycast(
                GetDetectionOrigin(),
                directionToTarget,
                distanceToTarget,
                obstacleMask,
                QueryTriggerInteraction.Ignore);

            if (blocked) continue;

            if (useCameraLogic)
            {
                float angleToTarget = Vector3.Angle(GetDetectionForward(), directionToTarget);
                if (angleToTarget > viewAngle / 2f) continue;

                // priorité au plus centré
                if (angleToTarget < bestScore)
                {
                    bestScore = angleToTarget;
                    bestShop = shop;
                    bestCollider = target;
                }
            }
            else
            {
                // fallback : priorité au plus proche
                if (distanceToTarget < bestScore)
                {
                    bestScore = distanceToTarget;
                    bestShop = shop;
                    bestCollider = target;
                }
            }
        }

        if (bestShop == null)
        {
            if (validShopTargets.Count > 0)
            {
                foreach (var col in validShopTargets)
                {
                    if (col != null && col.TryGetComponent(out ShopStand shop))
                        shop.ShopIndicatorToggle(false);
                }

                validShopTargets.Clear();
                UIManager.Instance.shopInteractDisplay.ClearShop();
            }

            return;
        }

        if (validShopTargets.Count == 0 || validShopTargets[0] != bestCollider)
        {
            // désactive les anciens indicators
            foreach (var col in validShopTargets)
            {
                if (col != null && col.TryGetComponent(out ShopStand oldShop))
                    oldShop.ShopIndicatorToggle(false);
            }

            validShopTargets.Clear();
            validShopTargets.Add(bestCollider);

            bestShop.ShopIndicatorToggle(true);

            UIManager.Instance.shopInteractDisplay.ShowShop(bestShop);
        }
    }

    private void PlayTargetDetectedSound()
    {
        //PlayerActionFMODManager.Instance.PlayStyleAction(PlayerActionFMOD.STYLE, targetDashCount);
        PlayerActionFMODManager.Instance.PlayPlayerAction(PlayerActionFMOD.DETECT);
    }

    private void PlayTargetPopSound()
    {
        PlayerActionFMODManager.Instance.PlayStyleAction(PlayerActionFMOD.STYLE, targetDashCount);
    }

    public void NotifyJumpTargetPopped(Collider target)
    {
        if (target != currentJumpTarget)
            return;

        PlayTargetPopSound();
        targetDashCount = Mathf.Clamp(targetDashCount + 1, 0, 5);
    }

    public void ClearNPCTargets()
    {
        for (int i = validNPCTargets.Count - 1; i >= 0; i--)
        {
            Collider col = validNPCTargets[i];
            if (col != null && col.TryGetComponent(out InteractableNPC npc))
            {
                npc.DisableVisual();
            }
        }

        validNPCTargets.Clear();

        UIManager.Instance.dialogInteractDisplay.ToggleInterface(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, jumpDetectionRange);
    }

    private void ResetTargetDashCount()
    {
        targetDashCount = 0;
    }
}
