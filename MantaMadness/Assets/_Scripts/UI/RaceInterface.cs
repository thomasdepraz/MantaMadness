using TMPro;
using UnityEngine;

public class RaceInterface : MonoBehaviour
{
    public GameObject container;
    public TextMeshProUGUI lapsText;
    public TextMeshProUGUI timerText;

    private Race currentRace;

    public void Start()
    {
        UIManager.Instance.raceInterface = this;
        Hide();
    }

    public void Show()
    {
        container.SetActive(true);
        enabled = true;
    }

    public void Hide()
    {
        container.SetActive(false);
        enabled = false;
    }

    public void Init(Race race)
    {
        currentRace = race;
        Show();
    }

    public void Updateœnterface()
    {
        if (currentRace == null)
            return;

        lapsText.text = $"{currentRace.CurrentLap} / {currentRace.MaxLaps}";
        timerText.text = TextUtility.GetPrettyTime(((ITimer)currentRace).GetTime());
    }

    public void Update()
    { 
        Updateœnterface();
    }

}
