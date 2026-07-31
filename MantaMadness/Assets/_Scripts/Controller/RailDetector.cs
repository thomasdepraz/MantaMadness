using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RailDetector : MonoBehaviour
{
    public SimpleController controller;
    private bool onRail;
    private bool onReaver;
    private bool onWaterfall;
    private bool transferLocked = false;

    [Header("Rail Transfer")]
    [SerializeField] float transferSphereRadius = 0.6f;
    [SerializeField] float transferDetectDistance = 6f;
    [SerializeField] float transferRayOffset = 1.2f;
    [SerializeField] LayerMask railLayer;
    [SerializeField] LayerMask obstacleLayer;

    [SerializeField] GameObject railPreviewPrefab;
    GameObject leftPreview;
    GameObject rightPreview;

    public Rail leftRailCandidate { get; private set; }
    public Rail rightRailCandidate { get; private set; }

    public Vector3 LeftHitPoint { get; private set; }
    public Vector3 RightHitPoint { get; private set; }

    public Vector3 railRaycastDir;


    void Start()
    {
        if (railPreviewPrefab != null)
        {
            leftPreview = Instantiate(railPreviewPrefab);
            rightPreview = Instantiate(railPreviewPrefab);

            leftPreview.SetActive(false);
            rightPreview.SetActive(false);
        }
    }

    bool IsRailBlocked(Collider railCollider)
    {
        Vector3 origin = GetComponent<Collider>().bounds.center;

        // vrai point du rail le plus proche du joueur
        Vector3 target = railCollider.ClosestPoint(origin);

        railRaycastDir = target - origin;
        float dist = railRaycastDir.magnitude;

        Debug.DrawRay(origin, railRaycastDir.normalized * dist, Color.green, 2f);


        if (Physics.Raycast(origin, railRaycastDir.normalized, out RaycastHit hit, dist, obstacleLayer))
        {
            Debug.Log("COLLIDER DETECTED: " + hit.collider.name);
            // si on touche autre chose que ce collider
            if (hit.collider != railCollider)
                return true;
        }


        return false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Rail rail) && onRail is false)
        {

            if (IsRailBlocked(other))
            {
                Debug.Log("RAIL BLOCKED");
                return;
            }


            if (rail.isRoadBorder == true && controller.strafRoutine != null || 
                rail.isRoadBorder == true && controller.boostRoutine != null || 
                rail.isRoadBorder == true && controller.State != ControllerState.SURFING)
            {
                if (controller.EnterRail(rail))
                {
                    onRail = true;

                    if (rail is RailTarget railTarget)
                    {
                        railTarget.OnPlayerEnteredRail(controller);
                    }
                }
            }

            else if(rail.isRoadBorder == false)
            {
                if (controller.EnterRail(rail))
                {
                    onRail = true;

                    if (rail is RailTarget railTarget)
                    {
                        railTarget.OnPlayerEnteredRail(controller);
                    }
                }
            }

        }
        else if(other.TryGetComponent(out WaterFall waterfall) && onWaterfall is false)
        {
            if (controller.EnterWaterfall(waterfall))
            {
                onWaterfall = true;
            }
        }

        else if(other.TryGetComponent(out ReaverBoost reaver) && onReaver is false)
        {
            Debug.Log("REAVER BOOST DETECTED");
            if (controller.EnterReaverBoost(reaver))
            {
                Debug.Log("REAVER BOOST ENTERED");
                onReaver = true;
            }
        }
    }


    void Update()
    {
        if (controller.State == ControllerState.RAIL)
        {
            DetectRailTransfers();
        }
        else
        {
            if(!transferLocked)
            HidePreviews();
        }
    }

    Coroutine coroutine;
    public void ExitRail()
    {
        if(coroutine == null)
            coroutine = StartCoroutine(Cooldown());
    }

    public void ExitWaterfall()
    {
        if(coroutine == null)
            coroutine = StartCoroutine(Cooldown());
    }

    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(0.2f);
        onRail = false;
        onWaterfall = false;
        onReaver = false;
        coroutine = null;
    }
    public void ExitReaver()
    {
        if (coroutine == null)
            coroutine = StartCoroutine(Cooldown());
    }

    void DetectRailTransfers()
    {
        if (transferLocked)
            return;

        leftRailCandidate = null;
        rightRailCandidate = null;

        Vector3 origin = controller.transform.position + controller.hoverBehaviour.normalContainer.forward * transferRayOffset;
        Vector3 rightDir = controller.hoverBehaviour.normalContainer.right;

        Debug.DrawRay(origin, rightDir * transferDetectDistance, Color.blue);
        Debug.DrawRay(origin, -rightDir * transferDetectDistance, Color.red);

        int mask = railLayer | obstacleLayer;

        // RIGHT
        if (Physics.SphereCast(
            origin,
            transferSphereRadius,
            rightDir,
            out RaycastHit hitRight,
            transferDetectDistance,
            mask,
            QueryTriggerInteraction.Collide))
        {
            // si on touche un obstacle en premier on ignore
            if (((1 << hitRight.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                // obstacle touché avant un rail
            }
            else if (hitRight.collider.TryGetComponent(out Rail rail) && rail != controller.CurrentRail)
            {
                rightRailCandidate = rail;
                RightHitPoint = hitRight.point;
                ShowPreview(rightPreview, hitRight.point);
            }
        }

        // LEFT
        if (Physics.SphereCast(
            origin,
            transferSphereRadius,
            -rightDir,
            out RaycastHit hitLeft,
            transferDetectDistance,
            mask,
            QueryTriggerInteraction.Collide))
        {
            if (((1 << hitLeft.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                // obstacle touché avant un rail
            }
            else if (hitLeft.collider.TryGetComponent(out Rail rail) && rail != controller.CurrentRail)
            {
                leftRailCandidate = rail;
                LeftHitPoint = hitLeft.point;
                ShowPreview(leftPreview, hitLeft.point);
            }
        }

        if (!transferLocked)
        {
            if (leftPreview != null)
                leftPreview.SetActive(leftRailCandidate != null);

            if (rightPreview != null)
                rightPreview.SetActive(rightRailCandidate != null);
        }
    }

    void ShowPreview(GameObject preview, Vector3 pos)
    {
        preview.SetActive(true);
        preview.transform.position = pos;
    }
    void HidePreviews()
    {
        if (leftPreview != null) leftPreview.SetActive(false);
        if (rightPreview != null) rightPreview.SetActive(false);
    }
    public void ConfirmTransfer(bool toRight)
    {
        transferLocked = true;

        if (toRight)
        {
            if (leftPreview != null)
                leftPreview.SetActive(false);

            HighlightPreview(rightPreview);
        }
        else
        {
            if (rightPreview != null)
                rightPreview.SetActive(false);

            HighlightPreview(leftPreview);
        }

    }

    void HighlightPreview(GameObject preview)
    {
        if (preview == null) return;

        preview.transform.localScale = Vector3.one * 1.2f;
    }

    public void ResetTransferPreview()
    {
        transferLocked = false;

        if (leftPreview != null)
            leftPreview.SetActive(false);

        if (rightPreview != null)
            rightPreview.SetActive(false);
    }

    void OnDrawGizmos()
    {
        if (controller == null || controller.hoverBehaviour == null)
            return;

        Vector3 origin = controller.transform.position + controller.hoverBehaviour.normalContainer.forward * transferRayOffset;
        Vector3 rightDir = controller.hoverBehaviour.normalContainer.right;

        Vector3 rightEnd = origin + rightDir * transferDetectDistance;
        Vector3 leftEnd = origin - rightDir * transferDetectDistance;

        // couleur origine
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(origin, 0.1f);

        // RIGHT CAST
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(origin, transferSphereRadius);
        Gizmos.DrawWireSphere(rightEnd, transferSphereRadius);
        Gizmos.DrawLine(origin, rightEnd);

        // LEFT CAST
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, transferSphereRadius);
        Gizmos.DrawWireSphere(leftEnd, transferSphereRadius);
        Gizmos.DrawLine(origin, leftEnd);

        Vector3 origin2 = transform.position + Vector3.up * 0.5f;

        ////RAYCAST DETECTION RAIL
        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(origin2, origin2 + railRaycastDir);
    }
}
