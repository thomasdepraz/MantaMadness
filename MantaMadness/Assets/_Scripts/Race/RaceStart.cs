using TMPro;
using UnityEngine;

public class RaceStart : MonoBehaviour
{
    [Header("Parameters")]
    public Race raceToStart;
    public float waitTime;

    private float currentWaitTime;
    private RaceManager raceManager;


    private void Awake()
    {
        enabled = false;
    }

    private void Start()
    {
        raceManager = Game.Instance.raceManager;
        raceManager.raceStarted += RaceStarted;
        raceManager.raceEnded += RaceEnded;
    }

    public void Update()
    {
        if(currentWaitTime < waitTime)
        {
            currentWaitTime += Time.deltaTime;
        }
        else
        {
            raceManager.StartRace(raceToStart);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out SimpleController controller))
        {
            enabled = true;
            currentWaitTime = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.TryGetComponent(out SimpleController controller))
        {
            currentWaitTime = 0;
            enabled = false;
        }
    }

    private void RaceStarted()
    {
        gameObject.SetActive(false);
    }

    private void RaceEnded()
    {
        gameObject.SetActive(true);
    }
}
