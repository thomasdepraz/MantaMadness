using UnityEngine;

public class HoverBehaviour : MonoBehaviour
{
    public Transform normalContainer;
    private ControllerData m_data;
    private Rigidbody m_rigidbody;
    public LayerMask layerMask;

    public void Initialize(ControllerData data, Rigidbody rigidbody)
    {
        m_data = data;
        m_rigidbody = rigidbody;
    }

    public void Hover(RaycastHit hitInfo, float deltaTime)
    {
        Vector3 velocity = m_rigidbody.linearVelocity;
        Vector3 rayDir = -normalContainer.up;

        Vector3 otherVelocity = Vector3.zero;
        Rigidbody otherRb = hitInfo.rigidbody;
        if (otherRb != null)
        {
            otherVelocity = otherRb.linearVelocity;
        }

        float rayDirVel = Vector3.Dot(rayDir, velocity);
        float otherDirVel = Vector3.Dot(rayDir, otherVelocity);

        float relativeVelocity = rayDirVel - otherDirVel;
        float x = hitInfo.distance - m_data.hoverHeight;
        float springForce = (x * m_data.hoverStrength) - (relativeVelocity * m_data.hoverDamper);
        m_rigidbody.AddForce(rayDir * springForce, ForceMode.VelocityChange);

        Vector3 front = normalContainer.position + normalContainer.forward * 1f;
        Vector3 back = normalContainer.position - normalContainer.forward * 0.5f;

        bool frontHit = Physics.Raycast(front, rayDir, out RaycastHit frontInfo, m_data.hoverRaycastLength, layerMask, QueryTriggerInteraction.UseGlobal);
        bool backHit = Physics.Raycast(back, rayDir, out RaycastHit backInfo, m_data.hoverRaycastLength, layerMask, QueryTriggerInteraction.UseGlobal);

        Debug.DrawRay(frontInfo.point, frontInfo.normal * 3, Color.red);
        Debug.DrawRay(backInfo.point, backInfo.normal * 3, Color.red);
        Debug.DrawRay(hitInfo.point, hitInfo.normal * hitInfo.distance, Color.cyan);

        if (frontHit && backHit)
        {
            Vector3 averageNormal = (frontInfo.normal + backInfo.normal) * 0.5f;
            normalContainer.up = Vector3.Lerp(normalContainer.up, averageNormal, Time.deltaTime * m_data.hoverAlignementSpeed);
            normalContainer.Rotate(0, transform.eulerAngles.y, 0);
        }
    }

    public void ResetRotation(float deltaTime)
    {
        normalContainer.up = Vector3.up;
        normalContainer.Rotate(0, transform.eulerAngles.y, 0);
    }
}
