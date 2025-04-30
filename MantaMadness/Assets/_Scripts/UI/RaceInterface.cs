using TMPro;
using UnityEngine;

public class RaceInterface : MonoBehaviour
{
    public GameObject container;
    public TextMeshProUGUI lapsText;

    private Race currentRace;
    private bool isActive = false;

    private void Awake()
    {
        Hide();
    }

    public void Start()
    {
        UIManager.Instance.raceInterface = this;
    }

    public void Show()
    {
        container.SetActive(true);
        isActive = true;
    }

    public void Hide()
    {
        container.SetActive(false);
        isActive = false;
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
    }

    public void Update()
    {
        if (isActive)
            Updateœnterface();
    }

}
