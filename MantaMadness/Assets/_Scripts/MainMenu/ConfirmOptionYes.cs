using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ConfirmOptionYes : ConfirmOption
{
    public override void Submit()
    {
        DataPersistenceManager.Instance.DeleteSave();
        DataPersistenceManager.Instance.NewGame(forceNewGame: true);
        StartCoroutine(MainMenu.instance.LoadMainCoroutine());
        MainMenu.instance.PlaySound(MainMenu.instance.startGameSound);
    }
}

