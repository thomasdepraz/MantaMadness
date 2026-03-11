using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RailDetector : MonoBehaviour
{
    public SimpleController controller;
    private bool onRail;
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

    public void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Rail rail) && onRail is false)
        {
            if(rail.isRoadBorder == true && controller.strafRoutine != null || 
                rail.isRoadBorder == true && controller.boostRoutine != null || 
                rail.isRoadBorder == true && controller.State != ControllerState.SURFING)
            {
                if (controller.EnterRail(rail))
                {
                    onRail = true;
                }
            }

            else if(rail.isRoadBorder == false)
            {
                if (controller.EnterRail(rail))
                {
                    onRail = true;
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
        coroutine = null;
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

        // RIGHT
        if (Physics.SphereCast(
            origin,
            transferSphereRadius,
            rightDir,
            out RaycastHit hitRight,
            transferDetectDistance,
            railLayer,
            QueryTriggerInteraction.Collide))
        {
            if (!IsPathBlocked(origin, hitRight.point))
            {
                if (hitRight.collider.TryGetComponent(out Rail rail) && rail != controller.CurrentRail)
                {
                    rightRailCandidate = rail;
                    RightHitPoint = hitRight.point;
                    ShowPreview(rightPreview, hitRight.point);
                }
            }
        }

        // LEFT
        if (Physics.SphereCast(
            origin,
            transferSphereRadius,
            -rightDir,
            out RaycastHit hitLeft,
            transferDetectDistance,
            railLayer,
            QueryTriggerInteraction.Collide))
        {
            if (!IsPathBlocked(origin, hitLeft.point))
            {
                if (hitLeft.collider.TryGetComponent(out Rail rail) && rail != controller.CurrentRail)
                {
                    leftRailCandidate = rail;
                    LeftHitPoint = hitLeft.point;
                    ShowPreview(leftPreview, hitLeft.point);
                }
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

    bool IsPathBlocked(Vector3 origin, Vector3 targetPoint)
    {
        Vector3 dir = targetPoint - origin;
        float dist = dir.magnitude;

        if (Physics.SphereCast(origin, 0.2f, dir.normalized, out _, dist, obstacleLayer))
            return true;

        return false;
    }

}
