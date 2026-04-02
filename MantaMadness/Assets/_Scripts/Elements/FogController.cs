using UnityEngine;

public class FogController : MonoBehaviour
{
    private SimpleController player;

    private void Start()
    {
        player = Game.Instance.player;
    }

    private void FixedUpdate()
    {
        MoveFog();   
    }

    private void MoveFog()
    {
        if(player != null)
        transform.position = player.transform.position;
    }
}
