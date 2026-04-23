using UnityEngine;

public class LavaZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out SimpleController controller))
        {
            if(controller.lavaResistanceAbility != true)
            {
                Game.Instance.player.Kill(DeathType.BURNED);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out SimpleController controller))
        {
            if (controller.lavaResistanceAbility != true)
            {
                Game.Instance.player.Kill(DeathType.BURNED);
            }
        }
    }
}
