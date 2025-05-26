using UnityEngine;

public class HoverBehaviour : MonoBehaviour
{
    public Transform normalContainer;
    private ControllerData m_data;
    private Rigidbody m_rigidbody;

    public void Initialize(ControllerData data, Rigidbody rigidbody)
    {
        m_data = data;
        m_rigidbody = rigidbody;
    }

    public void Hover(RaycastHit hitInfo, float deltaTime)
    {
        Vector3 velocity = m_rigidbody.linearVelocity;
        Vector3 rayDir = -transform.up;

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

        //transform.position = hitInfo.point + hitInfo.normal * m_data.hoverHeight; //new Vector3(transform.position.x, hitInfo.point.y + m_data.hoverHeight, transform.position.z);

        if (hitInfo.normal != Vector3.zero)
        {
            //Debug.Log(hitInfo.normal);
            //Quaternion target = Quaternion.LookRotation(normalContainer.transform.forward, hitInfo.normal);
            //normalContainer.transform.rotation = Quaternion.Lerp(normalContainer.transform.rotation, target, deltaTime * m_data.hoverAlignementSpeed);

            normalContainer.up = Vector3.Lerp(normalContainer.up, hitInfo.normal, Time.deltaTime * m_data.hoverAlignementSpeed);
            normalContainer.Rotate(0, transform.eulerAngles.y, 0);

        }
    }

    public void ResetRotation(float deltaTime)
    {
        //normalContainer.transform.rotation = Quaternion.RotateTowards(normalContainer.transform.rotation, Quaternion.FromToRotation(normalContainer.transform.up, Vector3.up), deltaTime * m_data.hoverAlignementSpeed);

        normalContainer.up = Vector3.up;
        normalContainer.Rotate(0, transform.eulerAngles.y, 0);
    }
}
