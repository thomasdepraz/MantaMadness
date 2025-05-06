using System;

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
