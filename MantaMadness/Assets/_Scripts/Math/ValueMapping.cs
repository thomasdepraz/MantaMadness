using UnityEngine;

public static class ValueMapping
{
    public static float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        float t = Mathf.InverseLerp(inMin, inMax, value);

        return Mathf.Lerp(outMin, outMax, t);
    }
}
