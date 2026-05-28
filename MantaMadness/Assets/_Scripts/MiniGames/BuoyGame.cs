using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Timeline.Actions.MenuPriority;

public class BuoyGame : MonoBehaviour, ITimer
{
    public static BuoyGame currentBuoyGame;

    public List<Buoy> buoys = new List<Buoy>();
    public float timeToFinish;
    public string coinName;
    
    private float timer;
    public bool hasStarted;

    public bool Completed { get => completed;}
    private bool completed = false;

    private HashSet<Buoy> collectedBuoys = new();

    void Start()
    {
        enabled = false;
        if(completed == false)
        {
            timer = 0;

            hasStarted = false;
        }
        for (int i = 0; i < buoys.Count; i++)
        {
            buoys[i].Initialize(this);
        }

        if (!ChallengeManager.instance.buoyGamesList.Contains(this))
        {
            ChallengeManager.instance.buoyGamesList.Add(this);
        }
    }

    public void StartGame()
    {
        if(currentBuoyGame != null && currentBuoyGame != this)
        {
            currentBuoyGame.Reset();
        }

        currentBuoyGame = this;
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
        if (!collectedBuoys.Add(collectedBuoy))
            return;

        if (!hasStarted)
            StartGame();

        Debug.Log($"{collectedBuoys.Count}/{buoys.Count}");

        if (collectedBuoys.Count >= buoys.Count)
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
        collectedBuoys.Clear();
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
        CoinManager.Instance.ActivateCoinHolder(coinName);
    }

    public override int GetHashCode()
    {
        return gameObject.name.GetHashCode();
    }

    public void Addtime(float time)
    {
        if(time != 0)
        {
            timer += time;
        }
    }
}
