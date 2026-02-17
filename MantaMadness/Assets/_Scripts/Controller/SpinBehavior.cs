using UnityEngine;

[RequireComponent (typeof(BoxCollider))]
public class SpinBehavior : MonoBehaviour
{

    [SerializeField] public bool spinColEnabled;
    [SerializeField] public bool spinBoostColEnabled;
    [SerializeField] private ParticleSystem spinParticle;

    private void Start()
    {
        spinParticle.gameObject.SetActive (false);
    }

    public void ToggleCollision(bool toggleValue)
    {
        spinColEnabled = toggleValue;

        if (toggleValue)
        {
            spinParticle.gameObject.SetActive(true);
            spinParticle.Play();
        }
        else
        {
            spinParticle.gameObject.SetActive(false);
            spinParticle.Stop();
        }
    }

    public void ToggleBoostCollision(bool toggleValue)
    {
        spinBoostColEnabled = toggleValue;
    }

    private void OnTriggerEnter(Collider other)
    {

        OnCollisionWithObject(other);
    }

    private void OnCollisionWithObject(Collider other)
    {
        if (spinColEnabled)
        {
            Debug.Log("Ca detecte bien");
            //Si de type DESTRUCTIBLE OBJECT
            //ACTIVé un behavior spécifique
        }
    }
}
