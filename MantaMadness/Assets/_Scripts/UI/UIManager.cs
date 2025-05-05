using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    [HideInInspector]
    public RaceInterface raceInterface;
    [HideInInspector]
    public TimerInterface miniGameTimerInterface;
}
