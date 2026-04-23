using UnityEngine;

public class ElectricalZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out SimpleController controller))
        {
                Game.Instance.player.Kill(DeathType.ELECTROCUTED);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent(out SimpleController controller))
        {
            Game.Instance.player.Kill(DeathType.ELECTROCUTED);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out SimpleController controller))
        {
                Game.Instance.player.Kill(DeathType.ELECTROCUTED);
        }
    }
}
