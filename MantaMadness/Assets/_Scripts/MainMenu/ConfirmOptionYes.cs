using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmOptionYes : ConfirmOption
{
    public override void Submit()
    {
        DataPersistenceManager.Instance.DeleteSave();
        DataPersistenceManager.Instance.NewGame(forceNewGame: true);
        SceneManager.LoadScene("Main");
    }
}

