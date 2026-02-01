using UnityEngine;

public class NPCQuestIndicator : MonoBehaviour
{
    [SerializeField] private InteractableNPC npc;

    [SerializeField] private GameObject visual;
    [SerializeField] private bool useNPCState;
    [SerializeField] private int requiredNPCState;

    private void Start()
    {
        npc.QuestMarker += DisableVisual;
    }

    private void OnEnable()
    {
        npc.QuestMarker += DisableVisual;
    }

    private void OnDisable()
    {
        npc.QuestMarker -= DisableVisual;
    }

    private void DisableVisual(int npcState)
    {
        if (npcState == requiredNPCState)
        {
            visual.SetActive(false);
        }
    }
}
