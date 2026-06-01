using FMODUnity;
using UnityEngine;

[System.Serializable]
public class AudioEmitterSettings
{
    public EventReference eventReference;

    [Header("3D")]
    public bool overrideAttenuation;
    public float minDistance = 1f;
    public float maxDistance = 20f;

    [Header("Startup Parameters")]
    public ParameterValue[] parameters;
}

[System.Serializable]
public struct ParameterValue
{
    public string parameterName;
    public float value;
}
