using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class InteractableNPC : MonoBehaviour, IDataPersistence
{
    [SerializeField] protected GameObject outOfRangeVisual;
    [SerializeField] protected GameObject inRangeVisual;
    [SerializeField] public string npcName;

    [SerializeField] protected NPCDialogState[] dialogStates;

    [SerializeField] protected int npcState = 0;
    public int NpcState => npcState;
    public int dialogIndex = 0;

    protected bool stateChangedDuringDialog = false;

    public virtual void Start()
    {
        if(inRangeVisual.activeSelf == true)
        {
            DisableVisual();
        }

        print(npcName);
    }

    public void LoadData(GameData data)
    {
        if (data.npcDialogData.TryGetValue(npcName, out NPCDialogData dialogData))
        {
            npcState = dialogData.npcState;
            dialogIndex = dialogData.dialogIndex;
        }

        OnDataLoaded();
    }

    public virtual void OnDataLoaded() { }

    public void SaveData(ref GameData data)
    {
        NPCDialogData dialogData = new NPCDialogData
        {
            npcState = npcState,
            dialogIndex = dialogIndex
        };

        if (data.npcDialogData.ContainsKey(npcName))
            data.npcDialogData[npcName] = dialogData;
        else
            data.npcDialogData.Add(npcName, dialogData);

    }

    public string[] GetCurrentDialogKeys()
    {
        foreach (var ds in dialogStates)
        {
            if (ds.state == npcState)
                return ds.dialogKeys;
        }
        return null;
    }

    public string GetCurrentDialogKey()
    {
        var dialogs = GetCurrentDialogKeys();

        if (dialogs == null || dialogs.Length == 0)
            return null;

        if (dialogIndex < 0 || dialogIndex >= dialogs.Length)
            return dialogs[dialogs.Length - 1]; // sécurité

        return dialogs[dialogIndex];
    }

    public void SetState(int newState)
    {
        npcState = newState;
        dialogIndex = 0; // reset quand on change de state
    }

    public void IncrementIndex()
    {
        var dialogs = GetCurrentDialogKeys();
        if (dialogs == null) return;

        if (dialogIndex < dialogs.Length - 1)
        {
            dialogIndex++;
            print("Current index is" + dialogIndex + "and current state is " + npcState);
        }
    }

    public void EnableVisual()
    {
        outOfRangeVisual.SetActive(false);
        inRangeVisual.SetActive(true);
    }

    public void DisableVisual()
    {
        outOfRangeVisual.SetActive(true);
        inRangeVisual.SetActive(false);
    }

    public virtual void OnDialogStarted()
    {
        DisableVisual();
    }

    public virtual void OnDialogFinished()
    {
        if (stateChangedDuringDialog)
        {
            stateChangedDuringDialog = false;
            return;
        }

        IncrementIndex();
    }

    public virtual void OnDialogStepReached(string sequenceKey,int dialogIndex) { }
}
