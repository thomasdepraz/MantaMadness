using UnityEngine;

public class MantaVisuals : MonoBehaviour
{
    SimpleController mantaController;

    public Transform modelTransform;

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

    private void UpdateModelRoll()
    {
        float angular = mantaController.AngularVelocity.y;

        modelTransform.localRotation = Quaternion.Euler(0, 0, -angular * 10);
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
