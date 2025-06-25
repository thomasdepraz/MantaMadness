using UnityEngine;

public class BillboardSprite : MonoBehaviour
{
    [SerializeField] private BillboardType billboardType;
    public enum BillboardType { LookAtCamera, CameraForward};

    private void LateUpdate()
    {
        switch (billboardType)
        {
            case BillboardType.LookAtCamera:
                transform.LookAt(Camera.main.transform.position, Vector3.up);
                break;
            case BillboardType.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            default:
                break;
        }
    }
}
