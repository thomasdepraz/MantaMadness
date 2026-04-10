using UnityEngine;

public class BreakableWall : MonoBehaviour
{
    [SerializeField] protected GameObject wall;
    [SerializeField] protected ParticleSystem breakParticle;

    protected bool isBroken = false;

    protected virtual void Start()
    {
        if (wall.activeSelf == false)
            wall.SetActive(true);

        if(breakParticle !=  null && wall != null)
        {
            var rend = breakParticle.GetComponent<ParticleSystemRenderer>();
            rend.material = wall.GetComponent<MeshRenderer>().material;
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if (isBroken == false && controller.Velocity.magnitude > controller.controllerData.maxSpeed)
            {
                isBroken = true;
                wall.SetActive(false);
                breakParticle.Play();
            }
        }
    }
}
