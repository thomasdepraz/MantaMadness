using UnityEngine;

public class WaterPhysicsTest : MonoBehaviour
{
    private Rigidbody rb;
    public float waterDrag;
    public float divingForce;
    public float depthMult;
    public float maxDivingDepth;
    public float rayLength;
    public float hoverHeight;
    public float hoverStrength;
    public float hoverDamper;

    private WaterBlock currentWaterBlock;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    bool InAir;
    bool IsDiving;
    bool IsSurfing = true;
    bool AtMaxDepth = false;
    public void FixedUpdate()
    {
        bool hasHit = Physics.Raycast(transform.position, -transform.up, out RaycastHit info, rayLength);
        Debug.DrawRay(transform.position, -transform.up * rayLength, hasHit ? Color.green : Color.red);

        if (InAir || IsSurfing)
        {
            //Apply gravity
            rb.AddForce(-transform.up * 9.8f);
        }

        //if falling;
        if(InAir && IsSurfing == false && rb.linearVelocity.y < 0)
        {
            IsSurfing = hasHit;
        }

        if(IsSurfing)
        {
            InAir = false;
            AtMaxDepth = false;

            if(hasHit)
            {
                Vector3 velocity = rb.linearVelocity;
                Vector3 rayDir = -transform.up;

                Vector3 otherVelocity = Vector3.zero;
                Rigidbody otherRb = info.rigidbody;
                if(otherRb != null)
                {
                    otherVelocity = otherRb.linearVelocity;
                }

                float rayDirVel = Vector3.Dot(rayDir, velocity);
                float otherDirVel = Vector3.Dot(rayDir, otherVelocity);

                float relativeVelocity = rayDirVel - otherDirVel;
                float x = info.distance - hoverHeight;
                float springForce = (x * hoverStrength) - (relativeVelocity * hoverDamper);
                rb.AddForce(rayDir * springForce);

                //add force on touched object
                if(otherRb != null)
                {
                    otherRb.AddForceAtPosition(rayDir * -springForce, info.point);
                }
            }
            else
            {
                rb.linearVelocity = Vector3.zero;
            }
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.down * divingForce, ForceMode.Impulse);
            IsDiving = true;
            IsSurfing = false;
            AtMaxDepth = false;
        }

        if (currentWaterBlock == null)
            return;

        float currentDepth = currentWaterBlock.GetDepthAtPosition(transform.position, out _);
        if (currentDepth < 0)
            return;

        if(IsDiving)
        {
            if (currentDepth < maxDivingDepth)
            {
                //Drag on y velocity - the deeper the higher the drag -- IS THIS NECESARRY ?
                rb.AddForce(new Vector3(0.0f, -rb.linearVelocity.y, 0.0f) * (currentDepth / maxDivingDepth) * waterDrag * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
            else if (currentDepth > maxDivingDepth && AtMaxDepth == false)
            {
                //Stop when hitting max depth
                rb.AddForce(new Vector3(0.0f, -rb.linearVelocity.y, 0.0f), ForceMode.VelocityChange);
                AtMaxDepth = true;
            }
        }

        if(Input.GetKeyUp(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * currentDepth * depthMult, ForceMode.Impulse);
            IsDiving = false;
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(false == collision.gameObject.TryGetComponent<WaterBlock>(out WaterBlock waterBlock))
        {
            return;
        }
        currentWaterBlock = waterBlock;
    }


    private void OnTriggerExit (Collider collision)
    {
        if (collision.gameObject.TryGetComponent<WaterBlock>(out _))
            currentWaterBlock = null;

        InAir = true;
    }
}
