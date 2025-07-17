using UnityEngine;

public class SunRotation : MonoBehaviour
{
    private SimpleController player;

    private void Start()
    {
        player = Game.Instance.player;
    }

    void Update()
    {
        gameObject.transform.LookAt(player.transform);
    }
}
