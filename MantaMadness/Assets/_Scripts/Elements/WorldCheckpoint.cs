using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WorldCheckpoint : MonoBehaviour
{
    [SerializeField] public Transform respawnTransform;
    [SerializeField] public string indexName;
    [SerializeField] public bool displayAreaName;
    [SerializeField] public string nameToDisplay;

    private void Start()
    {
        if (!WorldCheckpointManager.Instance.checkpoints.Contains(this))
        {
            WorldCheckpointManager.Instance.checkpoints.Add(this);
        }

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
