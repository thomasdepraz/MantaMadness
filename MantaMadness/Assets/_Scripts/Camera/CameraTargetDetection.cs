using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using Unity.VisualScripting;
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

    [Header("NPC Target Detection")]
    [SerializeField] private float npcDetectionRange;

    [Header("Shop Target Detection")]
    [SerializeField] private float shopDetectionRange;
    [SerializeField] private LayerMask shopTargetMask;

    [Header("Parameters")]
    [SerializeField] private float viewAngle;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask jumpargetMask;
    [SerializeField] private LayerMask npcTargetMask;

    [Header("Target Lists")]
    [SerializeField] public List<Collider> validJumpTargets = new List<Collider>();
    [SerializeField] public List<Collider> validNPCTargets = new List<Collider>();
    [SerializeField] public List<Collider> validShopTargets = new List<Collider>();

    [SerializeField] private EventReference addTargetSound;

    [SerializeField] private List<CinemachineCamera> playerCameras = new List<CinemachineCamera>();

    private CinemachineBrain brain;



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
        shopDetectionRange = Game.Instance.player.controllerData.npcInteractionRadius;

        brain = Camera.main.GetComponent<CinemachineBrain>();
        viewAngle = Camera.main.fieldOfView;
    }

    private void LateUpdate()
    {
        Physics.SyncTransforms();

        DetectJumpTargets();
        DetectNPCTargets();
        DetectShopTargets();
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
    private float GetCurrentViewAngle()
    {
        if (Camera.main != null)
            return Camera.main.fieldOfView;

        return viewAngle;
    }

    private Vector3 GetDetectionOrigin()
{
    if (playerTransform != null)
        return playerTransform.position;

    return transform.position;
}

private Vector3 GetDetectionForward()
{
    if (IsPlayerCameraActive() && Camera.main != null)
        return Camera.main.transform.forward;

    if (playerTransform != null)
        return playerTransform.forward;

    return transform.forward;
}

    void DetectJumpTargets()
    {
        float addRange = jumpDetectionRange - jumpRangeBuffer;
        float removeRange = jumpDetectionRange + jumpRangeBuffer;
        float activationRange = jumpDetectionRange * activationRangeMultiplier;

        Collider[] targetsInActivationRange = Physics.OverlapSphere(
            transform.position,
            activationRange,
            jumpargetMask);

        // --- CLEAN VALID LIST (hysteresis remove) ---
        for (int i = validJumpTargets.Count - 1; i >= 0; i--)
        {
            Collider col = validJumpTargets[i];
            if (col == null)
            {
                validJumpTargets.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(transform.position, col.transform.position);

            if (dist > removeRange)
            {
                validJumpTargets.RemoveAt(i);

                JumpTarget jt = col.GetComponent<JumpTarget>();
                if (jt != null)
                    jt.SetVisualState(JumpTargetVisualState.OutOfRange, 0f);
            }
        }

        // --- MAIN LOOP ---
        foreach (Collider target in targetsInActivationRange)
        {
            JumpTarget jumpTar = target.GetComponent<JumpTarget>();
            if (jumpTar == null)
                continue;

            if (!jumpTar.isAvailable)
            {
                jumpTar.SetVisualState(JumpTargetVisualState.Inactive);
                continue;
            }

            float distanceToTarget =
                Vector3.Distance(transform.position, target.transform.position);

            // ----------------------------
            // 1️⃣ Hors activation range
            // ----------------------------
            if (distanceToTarget > activationRange)
            {
                jumpTar.SetVisualState(JumpTargetVisualState.OutOfRange, 0f);
                continue;
            }

            // ----------------------------
            // 2️⃣ Dans activation range
            // ----------------------------
            Vector3 directionToTarget =
                (target.transform.position - transform.position).normalized;

            float angleToTarget =
                Vector3.Angle(transform.forward, directionToTarget);

            bool inFOV = angleToTarget < viewAngle / 2f;
            bool blocked =
                Physics.Raycast(transform.position,
                                directionToTarget,
                                distanceToTarget,
                                obstacleMask);


            bool inAddRange = distanceToTarget <= addRange;

            // ----------------------------
            // 3️⃣ VALID
            // ----------------------------
            if (inAddRange && inFOV && !blocked)
            {
                if (!validJumpTargets.Contains(target))
                {
                    validJumpTargets.Add(target);
                    RuntimeManager.PlayOneShot(addTargetSound, Camera.main.transform.position);
                }

                jumpTar.SetVisualState(JumpTargetVisualState.InRange, 1f);
            }
            else
            {
                if (validJumpTargets.Contains(target))
                    validJumpTargets.Remove(target);

                // ----------------------------
                // 4️⃣ APPROACHING
                // ----------------------------
                if (distanceToTarget <= jumpDetectionRange + approachingRange)
                {
                    float approachingStart = jumpDetectionRange + approachingRange;
                    float t = Mathf.InverseLerp(approachingStart, addRange, distanceToTarget);
                    jumpTar.SetVisualState(JumpTargetVisualState.Approaching, t);
                }
                else
                {
                    // ----------------------------
                    // 5️⃣ OUT OF RANGE (but activated)
                    // ----------------------------
                    jumpTar.SetVisualState(JumpTargetVisualState.OutOfRange, 0f);
                }
            }
        }
    }



    void DetectNPCTargets()
    {
        if (DialogManager.instance.currentSequence != null)
        {
            ClearNPCTargets();
            return;
        }

        bool useCameraLogic = IsPlayerCameraActive();

        Collider[] targetsInRange = Physics.OverlapSphere(GetDetectionOrigin(), npcDetectionRange, npcTargetMask);

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

}
