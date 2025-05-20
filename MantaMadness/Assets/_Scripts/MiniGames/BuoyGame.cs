using System.Collections.Generic;
using UnityEngine;

public class BuoyGame : MonoBehaviour, ITimer, ICoinObjective, ISaveable
{
    public List<Buoy> buoys = new List<Buoy>();
    public float timeToFinish;
    
    private float timer;
    private int count = 0;
    private bool hasStarted;

    public bool Completed { get => completed;}
    private bool completed = false;

    public Coin coin;
    public Coin coinToUnlock => coin;

    bool ISaveable.CanSave => true;


    void Start()
    {
        enabled = false;
        for (int i = 0; i < buoys.Count; i++)
        {
            buoys[i].Initialize(this);
        }
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
        completed = true;
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

    void ISaveable.Save()
    {
        PlayerPrefs.SetInt(Constants.c_MiniGamePrefixSave + GetHashCode().ToString(), completed ? 1 : 0);
    }

    void ISaveable.Load()
    {
        completed = PlayerPrefs.GetInt(Constants.c_MiniGamePrefixSave + GetHashCode().ToString(), 0) == 0 ? false : true;
    }

    public override int GetHashCode()
    {
        return gameObject.name.GetHashCode();
    }
}
