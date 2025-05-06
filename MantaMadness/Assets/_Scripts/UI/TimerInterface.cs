using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class TimerInterface : MonoBehaviour, IScreen
{
    public GameObject Container { get => container; }
    public GameObject container;
    public GameObject timerPanel;
    public TextMeshProUGUI timerText;
    public ITimer currentTimer;


    public void SetTimer(ITimer timer)
    {
        currentTimer = timer;
    }

    public void Start()
    {
        enabled = false;
        UIManager.Instance.miniGameTimerInterface = this;
    }

    public void Update()
    {
        if(currentTimer != null)
        {
            var timeSpan = TimeSpan.FromSeconds(currentTimer.GetTime());
            string s = timeSpan.Seconds < 10 ? "0"+ timeSpan.Seconds.ToString() : timeSpan.Seconds.ToString();
            string ms = timeSpan.Milliseconds < 10 ? "0" + timeSpan.Milliseconds.ToString() : timeSpan.Milliseconds.ToString();
            timerText.text = s + " : " + ms;
        }
    }

    void IScreen.Show()
    {
        timerPanel.transform.localScale = Vector3.zero;
        container.SetActive(true);
        timerPanel.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBounce);

        enabled = true;
    }

    void IScreen.Hide()
    {
        container.SetActive(false);
        enabled = false;
    }
}
