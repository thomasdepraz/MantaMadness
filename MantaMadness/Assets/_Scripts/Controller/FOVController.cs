using Unity.Cinemachine;
using UnityEngine;

public class FOVController : MonoBehaviour
{
    private CinemachineBrain brain;
    [Header("Parameters")]
    public float maxAvatarSpeed;
    public float maxFOV;
    public AnimationCurve FOVProgression;
    private float defaultFOV;
    CinemachineCamera current;
    float currentFOV;

    private SimpleController controller;
    private void Awake()
    {
        controller = GetComponent<SimpleController>();
    }

    void Start()
    {
        brain = Camera.main.gameObject.GetComponent<CinemachineBrain>();
        current = brain.ActiveVirtualCamera as CinemachineCamera;
        defaultFOV = current.Lens.FieldOfView;
        currentFOV = defaultFOV;
    }

    
    void Update()
    {
        Vector3 horizontalVel = controller.Velocity;
        horizontalVel.y = 0;

        float targetFOV = Mathf.Lerp(
           defaultFOV,
           maxFOV,
           Mathf.Clamp01(FOVProgression.Evaluate(horizontalVel.magnitude / maxAvatarSpeed)));

        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime);

        current.Lens.FieldOfView = currentFOV;
    }
}
