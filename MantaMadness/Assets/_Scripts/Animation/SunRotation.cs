using UnityEngine;

public class SunRotation : MonoBehaviour
{
    private SimpleController player;
    public Transform securityLookAt;

    private void Start()
    {
        if(Game.Instance != null)
        player = Game.Instance.player;
    }

    void Update()
    {
        if(player != null)
        gameObject.transform.LookAt(player.transform);
        else if(securityLookAt != null)
        {
            gameObject.transform.LookAt(securityLookAt);
        }
    }
}
