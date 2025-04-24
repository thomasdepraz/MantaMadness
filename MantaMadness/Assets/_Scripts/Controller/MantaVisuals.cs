using UnityEngine;

public class MantaVisuals : MonoBehaviour
{
    SimpleController mantaController;

    public Transform modelTransform;

    private void Awake()
    {
        mantaController = GetComponent<SimpleController>();
    }

    private void Update()
    {
        UpdateModelRoll();
    }

    private void UpdateModelRoll()
    {
        float angular = mantaController.AngularVelocity.y;

        modelTransform.localRotation = Quaternion.Euler(0, 0, -angular * 10);

    }
}
