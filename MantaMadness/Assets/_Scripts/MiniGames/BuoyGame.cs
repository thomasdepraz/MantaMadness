using System.Collections.Generic;
using UnityEngine;

public class BuoyGame : MonoBehaviour, ITimer, ICoinObjective
{
    public List<Buoy> buoys = new List<Buoy>();
    public float timeToFinish;
    
    private float timer;
    private int count = 0;
    private bool hasStarted;

    public Coin coin;
    public Coin coinToUnlock => coin;

    void Start()
    {
        enabled = false;
        for (int i = 0; i < buoys.Count; i++)
        {
            buoys[i].Initialize(this);
        }

        coin?.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        enabled = true;
        timer = timeToFinish;
        hasStarted = true;
        (UIManager.Instance.miniGameTimerInterface as IScreen).Show();
        UIManager.Instance.miniGameTimerInterface.SetTimer(this);
    }

    public void Update()
    {
        if (hasStarted == false)
            return;

        timer -= Time.deltaTime;
        if(timer <= 0)
        {
            Reset();
        }
    }

    public void Collect(Buoy collectedBuoy)
    {
        if (hasStarted == false)
            StartGame();

        count++;
        if (count >= buoys.Count)
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        enabled = false;
        (UIManager.Instance.miniGameTimerInterface as IScreen).Hide();
        UnlockCoin();
    }

    public void Reset()
    {
        hasStarted = false;
        count = 0;
        (UIManager.Instance.miniGameTimerInterface as IScreen).Hide();
        for (int i = 0; i < buoys.Count; i++)
        {
            buoys[i].Reset();
        }
    }

    float ITimer.GetTime()
    {
        return timer;
    }

    public void UnlockCoin()
    {
        coin?.gameObject.SetActive(true);

        //do camera event ?
    }
}
