using UnityEngine;

public class WorldCheckpoint : MonoBehaviour
{
    [SerializeField] private Transform respawnTransform;
    [SerializeField] public string indexName;
    [SerializeField] private bool displayAreaName;
    [SerializeField] private string nameToDisplay;


    private void Start()
    {
        if(gameObject.GetComponent<MeshRenderer>().enabled == true)
        {
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out SimpleController controller))
        {
            WorldCheckpointManager.Instance.SetCheckpoint(respawnTransform, indexName, displayAreaName, nameToDisplay);
        }
    }
}
