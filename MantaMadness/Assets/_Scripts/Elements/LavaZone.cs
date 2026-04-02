using UnityEngine;

public class LavaZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out SimpleController controller))
        {
            if(controller.lavaResistanceAbility != true)
            {
                Game.Instance.Respawn(out Game.Instance.m_SpawnPosition, out Game.Instance.m_SpawnRotation);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out SimpleController controller))
        {
            if (controller.lavaResistanceAbility != true)
            {
                Game.Instance.Respawn(out Game.Instance.m_SpawnPosition, out Game.Instance.m_SpawnRotation);
            }
        }
    }
}
