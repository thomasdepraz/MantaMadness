using System.Collections.Generic;
using UnityEngine;

public class WorldStateUpdateBox : MonoBehaviour, IDataPersistence
{
    [SerializeField] private int newWorldState;
    [SerializeField] private bool hasBeenActivated;
    [SerializeField] private string id;

    public void LoadData(GameData data)
    {
        if (data.worldStateUpdateBoxes.TryGetValue(id, out bool savedState))
        {
            hasBeenActivated = savedState;
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.worldStateUpdateBoxes.ContainsKey(id))
        {
            data.worldStateUpdateBoxes[id] = hasBeenActivated;
        }
        else
        {
            data.worldStateUpdateBoxes.Add(id, hasBeenActivated);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if (hasBeenActivated)
                return;

            hasBeenActivated = true;
            UpdateWorldState();
        }
    }

    private void UpdateWorldState()
    {
        Game.Instance.SetGameState(newWorldState);
    }

 
#if UNITY_EDITOR

    [ContextMenu("Generate GUID")]
    private void GenerateGUID()
    {
        id = System.Guid.NewGuid().ToString();
    }
#endif

}
