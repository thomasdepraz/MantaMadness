using UnityEngine;

public class MantaVisuals : MonoBehaviour
{
    SimpleController mantaController;

    [Header("Rotation parameters")]
    public Transform modelTransform;
    public float rotationSpeed;

    [Header("Parameters")]
    public ParticleSystem surfParticles;
  

    private void Awake()
    {
        mantaController = GetComponent<SimpleController>();
    }

    private void Update()
    {
        UpdateModelRoll();
        UpdateParticles();
    }
    Quaternion targetRotation;
    private void UpdateModelRoll()
    {
        targetRotation = Quaternion.identity;

        if(mantaController.State == ControllerState.SURFING || mantaController.State == ControllerState.SWIMMING)
        {
            float angular = mantaController.AngularVelocity.y;
            targetRotation = Quaternion.Euler(0, 0, -angular * 10);
        }

        //Falling

        //Diving
        if(mantaController.State == ControllerState.DIVING)
        {
            float ratio = mantaController.Velocity.y / -10;
            float maxPitch = Mathf.Lerp(0, 80f, ratio);
            targetRotation = Quaternion.Euler(maxPitch, 0, 0);
        }

        if (mantaController.State == ControllerState.JUMPING || mantaController.State == ControllerState.SWIMMING)
        {
            float ratio = mantaController.Velocity.y / 10;
            float maxPitch = Mathf.Lerp(0, -80f, Mathf.Clamp01(ratio));
            targetRotation = Quaternion.Euler(maxPitch, 0, 0);
        }

        modelTransform.localRotation = Quaternion.Lerp(modelTransform.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void UpdateParticles()
    {
        if(mantaController.State == ControllerState.SURFING && mantaController.HorizontalVelocity.magnitude > 0.5f)
        {
            if (!surfParticles.isPlaying)
                surfParticles.Play();
        }
        else
        {
            surfParticles.Stop();
        }
        
    }

}
