using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CameraTargetDetection : MonoBehaviour
{
    public static CameraTargetDetection Instance;

    [SerializeField] private float jumpDetectionRange;
    [SerializeField] private float activationRangeMultiplier = 1.8f;
    [SerializeField] private float approachingRange = 3f;
    [SerializeField] float jumpRangeBuffer =  0.75f;
    [SerializeField] private float npcDetectionRange;
    [SerializeField] private float viewAngle;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask jumpargetMask;
    [SerializeField] private LayerMask npcTargetMask;

    [SerializeField] public List<Collider> validJumpTargets = new List<Collider>();
    [SerializeField] public List<Collider> validNPCTargets = new List<Collider>();

    [SerializeField] private EventReference addTargetSound;


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        jumpDetectionRange = Game.Instance.player.controllerData.targetDetectionRadius;
        npcDetectionRange = Game.Instance.player.controllerData.npcInteractionRadius;
        viewAngle = Camera.main.fieldOfView;
    }

    //private void Update()
    //{
    //    DetectJumpTargets();
    //    DetectNPCTargets();
    //}
    private void LateUpdate()
    {
        Physics.SyncTransforms();

        DetectJumpTargets();
        DetectNPCTargets();
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
        if ((DialogManager.instance.currentSequence != null))
        {
            ClearNPCTargets();
            return;
        }

        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, npcDetectionRange, npcTargetMask);

        for (int i = validNPCTargets.Count - 1; i >= 0; i--)
        {
            Collider npc = validNPCTargets[i];
            if(npc == null || Vector3.Distance(transform.position, npc.transform.position) > npcDetectionRange || !npc.TryGetComponent<InteractableNPC>(out _))
            {
                validNPCTargets.RemoveAt(i);
                npc?.GetComponent<InteractableNPC>().DisableVisual();
                UIManager.Instance.dialogInteractDisplay.ToggleInterface(false);
                print(npc + "removed (out of range)");
            }
        }

        foreach (Collider target in targetsInRange)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            // Vérifie si la cible est dans le champ de vision
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);


            if (angleToTarget < viewAngle / 2f) // Si dans le FOV
            {
                Debug.Log("NPCs in range: " + targetsInRange.Length);

                RaycastHit hit;
                bool blocked = Physics.Raycast(
                    transform.position,
                    directionToTarget,
                    out hit,
                    distanceToTarget,
                    obstacleMask,
                    QueryTriggerInteraction.Ignore);


                if (blocked)
                {
                    Debug.Log("BLOCKED BY: " + hit.collider.name + " | layer: " + hit.collider.gameObject.layer);
                }
                // Vérifie qu’aucun obstacle ne bloque la vue
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask, QueryTriggerInteraction.Ignore))
                {
                    //Add to list
                    //Debug.Log("Objet VISIBLE : " + target.name);
                    if (!validNPCTargets.Contains(target))
                    {
                        validNPCTargets.Add(target);
                        target.GetComponent<InteractableNPC>().EnableVisual();
                        UIManager.Instance.dialogInteractDisplay.ToggleInterface(true);
                        print(target + "Has been added");
                    }
                }
                else
                {
                    //Remove from list
                    //Debug.Log("Objet CACHÉ : " + target.name);
                    if (validNPCTargets.Contains(target))
                    {
                        validNPCTargets.Remove(target);
                        target.GetComponent<InteractableNPC>().DisableVisual();
                        UIManager.Instance.dialogInteractDisplay.ToggleInterface(false);
                        print(target + "Has been removed");
                    }
                }
            }
            else
            {
                //Debug.Log(target.name + "is in range but not in view");
                if (validNPCTargets.Contains(target))
                {
                    validNPCTargets.Remove(target);
                    target.GetComponent<InteractableNPC>().DisableVisual();
                    UIManager.Instance.dialogInteractDisplay.ToggleInterface(false);
                    print(target + "Has been removed");
                }
            }
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
