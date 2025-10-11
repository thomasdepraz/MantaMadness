using UnityEngine;

public class AnimatedOneWayDoor : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject[] objectsToDeactivate;
    [SerializeField] private ParticleSystem[] particles;
    [SerializeField] private bool isOpen;

    private void Start()
    {
        if (isOpen)
        {
            VisualBehavior();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if(isOpen != true)
            {
                isOpen = true;
            }
        }
    }

    private void Update()
    {
        if (animator.GetBool("isOpen") != isOpen)
        {
            animator.SetBool("isOpen", isOpen);
        }
    }

    public void VisualBehavior()
    {
        // THIS IS TRIGGERED THROUGH ANIMATION EVENT !!
        if(particles.Length > 0)
        {
            foreach (ParticleSystem p in particles)
            {
                p.Play();
            }
        }

        if (objectsToDeactivate.Length > 0)
        {
            foreach (GameObject visual in objectsToDeactivate)
            {
                visual.SetActive(false);
            }
        }
    }
}
