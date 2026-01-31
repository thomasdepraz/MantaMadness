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

        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, jumpDetectionRange, jumpargetMask);

        for (int i = validJumpTargets.Count - 1; i >= 0; i--)
        {
            Collider npc = validJumpTargets[i];
            if (npc == null)
            {
                validJumpTargets.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(transform.position, npc.transform.position);

            if (dist > removeRange)
            {
                validJumpTargets.RemoveAt(i);
                npc?.GetComponent<JumpTarget>().SwitchIndicatorVisibility(false);
                print(npc + " removed (out of range hysteresis)");
            }
        }


        foreach (Collider target in targetsInRange)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            if (distanceToTarget > addRange)
                continue;

            // Vérifie si la cible est dans le champ de vision
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < viewAngle / 2f) // Si dans le FOV
            {
                // Vérifie qu’aucun obstacle ne bloque la vue
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    //Add to list
                    //Debug.Log("Objet VISIBLE : " + target.name);
                    if (!validJumpTargets.Contains(target))
                    {
                        validJumpTargets.Add(target);
                        RuntimeManager.PlayOneShot(addTargetSound, Camera.main.transform.position);
                        target.GetComponent<JumpTarget>().SwitchIndicatorVisibility(true);
                        print(target + "Has been added");
                    }
                }
                else
                {
                    //Remove from list
                    //Debug.Log("Objet CACHÉ : " + target.name);
                    if (validJumpTargets.Contains(target))
                    {
                        validJumpTargets.Remove(target);
                        target.GetComponent<JumpTarget>().SwitchIndicatorVisibility(false);
                        print(target + "Has been removed");
                    }
                }
            }
            else
            {
                //Debug.Log(target.name + "is in range but not in view");
                if (validJumpTargets.Contains(target))
                {
                    validJumpTargets.Remove(target);
                    target.GetComponent<JumpTarget>().SwitchIndicatorVisibility(false);
                    print(target + "Has been removed");
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
