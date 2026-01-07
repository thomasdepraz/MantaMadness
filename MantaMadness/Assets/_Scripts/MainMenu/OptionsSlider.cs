using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class OptionSlider : IOptionItem
{
    [SerializeField] private Slider slider;
    [SerializeField] private float step = 0.05f;
    [SerializeField] private string vcaPath;
    private VCA vca;

    [SerializeField] private string playerPrefsKey;
    [SerializeField] private float defaultValue = 1f;


    private void Start()
    {
        if (!vca.isValid())
            vca = RuntimeManager.GetVCA(vcaPath);
        LoadValue();
    }
    public override void Select()
    {
        base.Select();
        slider.interactable = false;
    }

    public override void Deselect()
    {
        base.Deselect();
        slider.OnDeselect(null);
    }

    public override void Increase()
    {
        if (!isEditing) return;

        slider.value = Mathf.Clamp01(slider.value + step);
        ApplyValue();
    }

    public override void Decrease()
    {
        if (!isEditing) return;

        slider.value = Mathf.Clamp01(slider.value - step);
        ApplyValue();
    }

    public override void Submit()
    {
        isEditing = !isEditing;
        slider.interactable = isEditing;
    }
    public override void Cancel()
    {
        if (!isEditing) return;

        isEditing = false;
        slider.interactable = false;
    }

    public override void ForceExitEdit()
    {
        base.ForceExitEdit();
        slider.interactable = false;
    }

    private void LoadValue()
    {
        float savedValue = PlayerPrefs.GetFloat(playerPrefsKey, defaultValue);
        slider.value = savedValue;
        vca.setVolume(savedValue);
    }

    private void ApplyValue()
    {
        float value = slider.value;
        vca.setVolume(value);
        PlayerPrefs.SetFloat(playerPrefsKey, value);
    }
}

