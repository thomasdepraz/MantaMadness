using Unity.Cinemachine;
using UnityEngine;

public class MantaCameraController : MonoBehaviour
{
    public float fallingSpeedThreshold;
    public CinemachineCamera followCamera;

    private SimpleController mantaController;

    private void Awake()
    {
        mantaController = GetComponent<SimpleController>();
    }

    private void Update()
    {
        if(mantaController.CurrentDepth == 0 && mantaController.Velocity.y < 0 && Mathf.Abs(mantaController.Velocity.y) > fallingSpeedThreshold)
        {
            followCamera.gameObject.SetActive(false);
        }
        else
        {
            followCamera.gameObject.SetActive(true);
        }
    }
}
