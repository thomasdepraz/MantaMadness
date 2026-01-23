using System.Net.NetworkInformation;
using UnityEngine;

[System.Serializable]
public class StateActivation
{
    public int requiredState;
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;
}

[System.Serializable]
public class DialogTrigger
{
    public string requiredSequenceKey;
    public int requiredNpcState;   
    public int dialogIndex;        
    public int stateToSet;         
    public GameObject[] enable;
    public GameObject[] disable;
}



public class InteractableNPCEnabler : InteractableNPC
{
    [SerializeField] private StateActivation[] activations;
    [SerializeField] private DialogTrigger[] dialogTriggers;


    public override void Start()
    {
        base.Start();
        ApplyStateActivation();
    }

    public override void OnDataLoaded()
    {
        ApplyStateActivation();
    }

    public override void OnDialogFinished()
    {
        base.OnDialogFinished();
        ApplyStateActivation();
    }

    private void ApplyStateActivation()
    {
        foreach (var activation in activations)
        {
            bool active = npcState >= activation.requiredState;

            foreach (var obj in activation.objectsToEnable)
                if (obj) obj.SetActive(active);

            foreach (var obj in activation.objectsToDisable)
                if (obj) obj.SetActive(!active);
        }
    }

    public override void OnDialogStepReached(string sequenceKey,int dialogIndex)
    {


        foreach (var t in dialogTriggers)
        {
            if (!string.IsNullOrEmpty(t.requiredSequenceKey) && t.requiredSequenceKey != sequenceKey) continue;
            if (npcState != t.requiredNpcState) continue;
            if (dialogIndex != t.dialogIndex) continue;

            // si tu veux que ça ne se répète pas, tu peux “avancer” l’état
            if (t.stateToSet > npcState)
            {
                SetState(t.stateToSet);
                stateChangedDuringDialog = true;
            }

            foreach (var go in t.enable)
                if (go) go.SetActive(true);

            foreach (var go in t.disable)
                if (go) go.SetActive(false);

            // derrière, on applique les règles persistantes basées sur le state
            ApplyStateActivation();
        }
    }
}