using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] private GameObject wall;
    [SerializeField] private ParticleSystem breakParticle;
    [SerializeField] private Collider hardCollider;

    private bool isBroken = false;

    private void Start()
    {
        if (wall.activeSelf == false)
            wall.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if (isBroken == false && controller.Velocity.magnitude > controller.controllerData.maxSpeed)
            {
                isBroken = true;
                wall.SetActive(false);
                breakParticle.Play();
                if (hardCollider != null)
                    hardCollider.enabled = false;
            }
        }
    }
}
