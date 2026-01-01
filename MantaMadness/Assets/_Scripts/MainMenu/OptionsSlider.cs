using UnityEngine;
using UnityEngine.UI;

public class OptionSlider : IOptionItem
{
    [SerializeField] private Slider slider;
    [SerializeField] private float step = 0.05f;

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
    }

    public override void Decrease()
    {
        if (!isEditing) return;

        slider.value = Mathf.Clamp01(slider.value - step);
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
}

