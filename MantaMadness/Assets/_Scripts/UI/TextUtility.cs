using System;

public class TextUtility
{
    public static string GetPrettyTime(float timeInSeconds, bool withMinutes = false)
    {
        TimeSpan span = TimeSpan.FromSeconds(timeInSeconds);
        string minutes = span.Minutes > 10 ? span.Minutes.ToString() : "0" + span.Minutes.ToString();
        string seconds = span.Seconds > 10 ? span.Seconds.ToString() : "0" + span.Seconds.ToString();
        string ms = span.Milliseconds.ToString();
        int length = 3 - ms.Length;
        for (int i = 0; i < length; i++)
        {
            ms = "0" + ms;
        }

        if(withMinutes)
            return minutes + " : " + seconds + " : " + ms.Substring(0, 2);
        else
            return seconds + " : " + ms.Substring(0, 2);
    }
}
