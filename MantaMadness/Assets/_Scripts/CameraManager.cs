using Unity.Cinemachine;
using Unity.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(CinemachineBrain))]
public class CameraManager: MonoBehaviour
{
    private Camera mainCamera;

    public static CameraManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        brain = gameObject.GetComponent<CinemachineBrain>();
        defaultBlend = brain.DefaultBlend;
        defaultCamera = brain.ActiveVirtualCamera as CinemachineCamera;
        mainCamera = gameObject.GetComponent<Camera>();
    }

    private CinemachineBrain brain;
    private CinemachineBlendDefinition defaultBlend;
    private CinemachineCamera defaultCamera;

    public void BlendToCamera(CinemachineCamera camera) => BlendToCamera(camera, brain.DefaultBlend);
    public void BlendToCamera(CinemachineCamera camera, CinemachineBlendDefinition blend)
    {
        brain.DefaultBlend = blend;
        camera.gameObject.SetActive(true);
        camera.Priority = int.MaxValue;
    }

    public void ResetCamera(CinemachineCamera camera)
    {
        brain.DefaultBlend = defaultBlend;
        camera.Priority = 0;
        camera.gameObject.SetActive(false);

        defaultCamera.gameObject.SetActive(true);
    }

    public void AddCameraToStack(Camera camera)
    {
        mainCamera.GetUniversalAdditionalCameraData().cameraStack.Add(camera);
    }
}
