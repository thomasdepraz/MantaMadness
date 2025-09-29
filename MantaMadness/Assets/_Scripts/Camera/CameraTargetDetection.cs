using UnityEngine;
using System.Collections.Generic;

public class CameraTargetDetection : MonoBehaviour
{
    public static CameraTargetDetection Instance;

    [SerializeField] private float detectionRange;
    [SerializeField] private float viewAngle;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask targetMask;

    [SerializeField] public List<Collider> validTargets = new List<Collider>();


    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        detectionRange = Game.Instance.player.controllerData.targetDetectionRadius + 5f;
        viewAngle = Camera.main.fieldOfView;
    }

    private void Update()
    {
        DetectTargets();
    }

    void DetectTargets()
    {
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, detectionRange, targetMask);

        foreach (Collider target in targetsInRange)
        {
            Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
            float distanceToTarget = Vector3.Distance(transform.position, target.transform.position);

            // Vérifie si la cible est dans le champ de vision
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < viewAngle / 2f) // Si dans le FOV
            {
                // Vérifie qu’aucun obstacle ne bloque la vue
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    //Add to list
                    //Debug.Log("Objet VISIBLE : " + target.name);
                    if (!validTargets.Contains(target))
                    {
                        validTargets.Add(target);
                        target.GetComponent<JumpTarget>().SwitchIndicatorVisibility(true);
                        print(target + "Has been added");
                    }
                }
                else
                {
                    //Remove from list
                    //Debug.Log("Objet CACHÉ : " + target.name);
                    if (validTargets.Contains(target))
                    {
                        validTargets.Remove(target);
                        target.GetComponent<JumpTarget>().SwitchIndicatorVisibility(false);
                        print(target + "Has been removed");
                    }
                }
            }
            else
            {
                //Debug.Log(target.name + "is in range but not in view");
                if (validTargets.Contains(target))
                {
                    validTargets.Remove(target);
                    target.GetComponent<JumpTarget>().SwitchIndicatorVisibility(false);
                    print(target + "Has been removed");
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

}
