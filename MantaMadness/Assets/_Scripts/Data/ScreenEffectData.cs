using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RenderFeatureToggle
{
    public string featureName;
    public bool isActive;

    public RenderFeatureToggle(string name, bool isActive)
    {
        this.featureName = name;
        this.isActive = isActive;
    }
}

[CreateAssetMenu(fileName = "ScreenEffectData", menuName = "Game Data/Screen Effect Data")]
[Serializable]
public class ScreenEffectData : ScriptableObject
{
    public List<RenderFeatureToggle> ScreenEffects =
        new List<RenderFeatureToggle>()
        {
            new RenderFeatureToggle("Pixelize", true),
            new RenderFeatureToggle("CRT", true)
        };
}
