using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class TimerInterface : MonoBehaviour, IScreen
{
    public UnityEngine.GameObject Container { get => container; }
    public UnityEngine.GameObject container;
    public UnityEngine.GameObject timerPanel;
    public TextMeshProUGUI timerText;
    public ITimer currentTimer;

    private bool withMinutes = false;

    public void SetTimer(ITimer timer)
    {
        currentTimer = timer;
        withMinutes = TimeSpan.FromSeconds(timer.GetTime()).Minutes >= 1;
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
            timerText.text = TextUtility.GetPrettyTime(currentTimer.GetTime(), withMinutes);
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
