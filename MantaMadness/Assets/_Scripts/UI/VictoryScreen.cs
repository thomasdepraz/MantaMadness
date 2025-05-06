using TMPro;
using UnityEngine;

public class VictoryScreen : MonoBehaviour, IScreen
{
    public GameObject Container { get => container; }
    public GameObject container;

    public TextMeshProUGUI timer;
    public TextMeshProUGUI deathCount;
    public TextMeshProUGUI coolScore;
    public TextMeshProUGUI finalRank;

    public void Start()
    {
        UIManager.Instance.victoryScreen = this;
    }

    public void Initialize(Race race)
    {
        //Get info from race
        timer.text = TextUtility.GetPrettyTime((race as ITimer).GetTime());
    }

}