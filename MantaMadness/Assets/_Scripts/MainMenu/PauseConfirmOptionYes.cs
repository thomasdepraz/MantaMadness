using UnityEngine;

public class PauseConfirmOptionYes : ConfirmOption
{
    public override void Submit()
    {
        PauseMenu.instance.Resume();
        PauseMenu.instance.unstuckMenu.Close();
        Game.Instance.UnstuckPlayer();
    }
}
