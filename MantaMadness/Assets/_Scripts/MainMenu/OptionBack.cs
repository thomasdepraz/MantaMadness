using UnityEngine;

public class OptionBack : IOptionItem
{
    public override void Select()
    {
        base.Select();
    }

    public override void Deselect()
    {
        base.Deselect();
    }
    public override void Increase() { }
    public override void Decrease() { }

    public override void Submit()
    {
        //MainMenu.instance.State = MainMenu.MainMenuState.DEFAULT;
        //MainMenu.instance.options.CloseFromMainMenu();
        //Deselect();
        OptionsMenu.instance.RequestClose();
        Deselect();
    }

    public override void Cancel()
    {
        Submit();
    }
}
