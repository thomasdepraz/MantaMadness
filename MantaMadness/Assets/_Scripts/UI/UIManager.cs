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
    public GameInterface gameInterface;
    [HideInInspector]
    public RaceInterface raceInterface;
    [HideInInspector]
    public TimerInterface miniGameTimerInterface;
    [HideInInspector]
    public VictoryScreen victoryScreen;
    [HideInInspector]
    public TransitionInterface transitionScreen;
    [HideInInspector]
    public BoostGaugeInterface boostGaugeInterface;
    [HideInInspector]
    public DialogInteractDisplay dialogInteractDisplay;
    [HideInInspector]
    public ShopInteractDisplay shopInteractDisplay;

    public void ToggleBaseInterface(bool toggle)
    {
        gameInterface.ToggleInterface(toggle);
        boostGaugeInterface.ToggleInterface(toggle);
        //dialogInteractDisplay.ToggleInterface(toggle);
    }
}
