using TMPro;
using UnityEngine;

public class RaceStart : MonoBehaviour
{
    [Header("Parameters")]
    public Race raceToStart;
    public float waitTime;
    public float timeToBeat;

    private float currentWaitTime;
    private RaceManager raceManager;


    public GameObject[] toActivateArray;
    public GameObject victoryCoin;

    private void Awake()
    {
        if (toActivateArray.Length > 0)
        {
            EnableRaceObjects(false);
        }
        else
        {
            print("Array List is empty");
        }

        if (victoryCoin != null)
        {
            victoryCoin.SetActive(false);
        }

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
        if (toActivateArray.Length > 0)
        {
            EnableRaceObjects(true);
        }
        else
        {
            print("Array List is empty");
        }
    }

    private void RaceEnded()
    {
        gameObject.SetActive(true);
        if (toActivateArray.Length > 0)
        {
            EnableRaceObjects(false);
        }
        else
        {
            print("Array List is empty");
        }
        if(victoryCoin != null)
        {
            victoryCoin.SetActive(true);
        }
    }

    private void EnableRaceObjects(bool toggle)
    {
        if(toggle == true)
        {
            foreach(GameObject objects in toActivateArray)
            {
                objects.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject objects in toActivateArray)
            {
                objects.SetActive(false);
            }
        }
    }
}
