using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Combo/Combo Database")]
public class ComboDatabase : ScriptableObject
{
    public List<ComboActionSO> actions;

    private Dictionary<ComboID, ComboActionSO> lookup;


    public void Awake()
    {
        Init();
    }

    public void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        lookup = new Dictionary<ComboID, ComboActionSO>();

        foreach (var action in actions)
        {
            if (!lookup.ContainsKey(action.id))
                lookup.Add(action.id, action);
            else
                Debug.LogWarning($"Duplicate ComboID: {action.id}");
        }
    }

    public ComboActionSO Get(ComboID id)
    {
        if (lookup == null)
            Init();

        lookup.TryGetValue(id, out var action);
        return action;
    }
}
