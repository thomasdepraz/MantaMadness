using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    public static NPCManager instance;

    public List<InteractableNPC> npcList;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void UpdateNPCState(string name, int stateIndex)
    {
        foreach(InteractableNPC npc in npcList)
        {
            if (npc.npcName == name)
            {
                Debug.Log(npc.npcName+ "Has been updated to state " + stateIndex);
                npc.SetState(stateIndex);
                return;
            }
        }
    }
}
