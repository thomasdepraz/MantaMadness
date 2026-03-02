using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum ComboEffectPreset
{
    None,
    sketchy,
    dangle,
    shear,
    wave,
    grow,
    swing,
    palette,
    spread,
    funky,
    jump,
    shake,
    fade,
    pivot,
    pivotc,
}
public enum ComboID
{
    Jump,
    Spin,
    Dash,
    GalaxySpin,
    DiveBoost,
    TargetJump,
    SpinBoost,
    SpinAirBoost,
    TornadoJump,
    GalaxyBoost,

}

[CreateAssetMenu(menuName = "Combo/Combo Action")]
public class ComboActionSO : ScriptableObject
{
    [Header("Identity")]
    public ComboID id;

    [Header("Core")]
    public string actionName;
    public int value;
    public ComboType type;

    [Header("TMPEffects Preset")]
    public ComboEffectPreset effectPreset;

    [Header("Effect Parameters")]
    [Range(0, 50)] public int amplitude = 10;
    [Range(0, 10)] public int speed = 1;
    [Range(0, 5)] public int frequency = 1;

    [Header("TMP Look")]
    public TMP_FontAsset fontOverride;
}
