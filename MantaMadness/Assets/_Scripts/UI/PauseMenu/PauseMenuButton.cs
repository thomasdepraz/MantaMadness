using UnityEngine;

public class PauseMenuButton : MonoBehaviour
{
    public GameObject cursor;

    public void EnableButton()
    {
        cursor.SetActive(true);
    }

    public void ResetButton()
    {
        cursor.SetActive(false);
    }
}
