using JetBrains.Annotations;
using System.Runtime.CompilerServices;
using UnityEngine;

public class FOVController : MonoBehaviour
{
    private Camera mainCamera;
    [Header("Parameters")]
    public float maxAvatarSpeed;
    public float maxFOV;
    public AnimationCurve FOVProgression;
    private float defaultFOV;

    private SimpleController controller;
    void Start()
    {
        mainCamera = Camera.main;
        controller = GetComponent<SimpleController>();
        defaultFOV = mainCamera.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        mainCamera.fieldOfView = Mathf.Lerp(
            defaultFOV, 
            maxFOV, 
            Mathf.Clamp01(FOVProgression.Evaluate(controller.Velocity.magnitude / maxAvatarSpeed)));   
    }
}
