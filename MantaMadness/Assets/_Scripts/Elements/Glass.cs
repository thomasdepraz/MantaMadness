using UnityEngine;

public class Glass : MonoBehaviour
{
    [SerializeField] private GameObject glass;
    [SerializeField] private ParticleSystem glassParticle;

    private bool isBroken = false;

    private void Start()
    {
        if(glass.activeSelf == false)
        glass.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if (isBroken == false)
            {
                isBroken = true;
                glass.SetActive(false);
                glassParticle.Play();
            }
        }
    }
}
