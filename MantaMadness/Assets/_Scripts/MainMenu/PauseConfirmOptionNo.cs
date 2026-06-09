using UnityEngine;

public class PauseConfirmOptionNo : ConfirmOption
{
    public override void Submit()
    {
        PauseMenu.instance.CloseUnstuck();
        PauseMenu.instance.unstuckMenu.Close();
    }
}
