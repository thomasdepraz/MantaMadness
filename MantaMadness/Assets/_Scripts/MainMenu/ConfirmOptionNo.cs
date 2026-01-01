using UnityEngine;

public class ConfirmOptionNo : ConfirmOption
{
    public override void Submit()
    {
        MainMenu.instance.State = MainMenu.MainMenuState.DEFAULT;
    }
}

