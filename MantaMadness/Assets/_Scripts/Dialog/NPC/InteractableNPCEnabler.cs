using UnityEngine;

[System.Serializable]
public class StateActivation
{
    public int npcState;
    public GameObject[] objectsToEnable;
}


public class InteractableNPCEnabler : InteractableNPC
{

    //Set d'objet a activé.
    [SerializeField] private StateActivation[] activations;

    public override void Start()
    {
        base.Start();
        ApplyStateActivation();
    }

    public override void OnDialogFinished()
    {
        base.OnDialogFinished();
        ApplyStateActivation();
    }

    public override void OnDataLoaded()
    {
        ApplyStateActivation();
    }

    private void ApplyStateActivation()
    {
        foreach (var activation in activations)
        {
            bool shouldBeActive = npcState >= activation.npcState;

            foreach (var obj in activation.objectsToEnable)
            {
                if (obj != null)
                    obj.SetActive(shouldBeActive);
            }
        }
    }
}
