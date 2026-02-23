using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

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

    private void OnTriggerStay(Collider other)
    {
        OnCollisionWithObject(other);
    }

    private void OnCollisionWithObject(Collider other)
    {
        if (!spinColEnabled)
            return;


        if (other.GetComponent<DestructibleCollisionRelay>() != null)
            return;

        if (other.gameObject.layer != LayerMask.NameToLayer("Wall"))
            return;

        float distance = Vector3.Distance(transform.position, other.ClosestPoint(transform.position));

#if UNITY_EDITOR
        EditorGUIUtility.PingObject(other.gameObject);
        Selection.activeGameObject = other.gameObject;
#endif

        Debug.Log("Collided with: " + other.name);

        Vector3 closestPoint = other.ClosestPoint(transform.position);
        Vector3 normal = (transform.position - closestPoint).normalized;
        MantaVisuals.instance.SpawnSpinImpactParticles(closestPoint);
        Game.Instance.player.SpinBounce(normal);
    }
}
