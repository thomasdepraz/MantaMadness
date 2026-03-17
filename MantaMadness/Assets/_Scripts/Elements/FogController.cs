using UnityEngine;

public class FogController : MonoBehaviour
{
    private SimpleController player;

    [SerializeField] private Material[] fogMat;

    private void Start()
    {
        player = Game.Instance.player;
    }

    //private void OnEnable()
    //{
        
    //}

    //private void OnDisable()
    //{
        
    //}

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
