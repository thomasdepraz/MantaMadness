using UnityEngine;

public class EventDisableZoneAction : MonoBehaviour, IButtonAction
{
    [SerializeField] private EventCheckpoint checkpoint;

    public void Execute()
    {
        checkpoint.DisableZone();
    }
}
