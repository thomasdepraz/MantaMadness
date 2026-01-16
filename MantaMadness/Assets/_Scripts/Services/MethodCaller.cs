using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public class MethodCaller : MonoBehaviour
{
    [Header("Event à écouter")]
    public UnityEvent listenTo;

    [Header("Actions secondaires")]
    public UnityEvent onTrigger;

    private void OnEnable()
    {
        listenTo.AddListener(Trigger);
    }

    private void OnDisable()
    {
        listenTo.RemoveListener(Trigger);
    }

    public void Trigger()
    {
        onTrigger.Invoke();
    }
}
