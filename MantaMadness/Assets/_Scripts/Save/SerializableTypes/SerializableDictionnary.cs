using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionnary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    //Save dictionnary to list
    public void OnBeforeSerialize()
    {
        keys.Clear(); 
        values.Clear();
        foreach(KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    //Load dictionnary from list 
    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
        {
            Debug.LogError("Tried to deserialize a Serializable Dictionnary but the amount of keys (" + keys.Count + ") " +
                "does not match the number of values  (" + values.Count + ") which means something went wrong");
        }

        for (int i = 0; i < keys.Count; i++)
        {
            this.Add(keys[i], values[i]);
        }
    }
}
