using UnityEngine;
using UnityEngine.UI;

public class OptionToggle : IOptionItem
{
    [SerializeField] private Toggle toggle;

    public override void Select()
    {
        base.Select();
        toggle.Select();
    }

    public override void Deselect()
    {
        base.Deselect();
        toggle.OnDeselect(null);
    }

    public override void Increase() => toggle.isOn = true;
    public override void Decrease() => toggle.isOn = false;
    public override void Submit() => toggle.isOn = !toggle.isOn;
    public override void Cancel() { }
    public override void ForceExitEdit()
    {
        base.ForceExitEdit();
        toggle.interactable = false;
    }
}

