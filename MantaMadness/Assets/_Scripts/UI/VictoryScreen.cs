using System;
using TMPro;
using UnityEngine;

public class TextUtility
{
    public static string GetPrettyTime(float timeInSeconds)
    {
        TimeSpan span = TimeSpan.FromSeconds(timeInSeconds);
        string seconds = span.Seconds > 10 ? span.Seconds.ToString() : "0" + span.Seconds.ToString();
        string ms = span.Milliseconds.ToString();
        int length = 4 - ms.Length;
        for (int i = 0; i < length; i++)
        {
            ms = "0" + ms;
        }
        return seconds + " : " + ms.Substring(0, 2);
    }
}

public class VictoryScreen : MonoBehaviour, IScreen
{
    public GameObject Container { get => container; }
    public GameObject container;

    public TextMeshProUGUI timer;
    public TextMeshProUGUI deathCount;
    public TextMeshProUGUI coolScore;
    public TextMeshProUGUI finalRank; 

    public void Initialize(Race race)
    {
        //Get info from race
        timer.text = TextUtility.GetPrettyTime((race as ITimer).GetTime());
    }

}