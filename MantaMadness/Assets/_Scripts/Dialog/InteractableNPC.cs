using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class InteractableNPC : MonoBehaviour, IDataPersistence
{
    [SerializeField] private GameObject outOfRangeVisual;
    [SerializeField] private GameObject inRangeVisual;
    [SerializeField] public string npcName;

    [SerializeField] private NPCDialogState[] dialogStates;

    [SerializeField] private int npcState = 0;
    public int dialogIndex = 0;

    private void Start()
    {
        if(inRangeVisual.activeSelf == true)
        {
            DisableVisual();
        }
    }

    public void LoadData(GameData data)
    {
        if (data.npcDialogData.TryGetValue(npcName, out NPCDialogData dialogData))
        {
            npcState = dialogData.npcState;
            dialogIndex = dialogData.dialogIndex;
        }
    }

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

        //if (data.npcDialogState.ContainsKey(npcName))
        //{
        //    data.npcDialogState.Remove(npcName);
        //}
        //data.npcDialogState.Add(npcName, dialogIndex);
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
}
