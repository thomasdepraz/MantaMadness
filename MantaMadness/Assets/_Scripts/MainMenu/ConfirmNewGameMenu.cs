using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfirmNewGameMenu : MonoBehaviour
{
    [SerializeField] private GameObject[] visuals;
    [SerializeField] private ConfirmOption[] options;

    private int currentIndex;

    public void Start()
    {
        ToggleVisuals(false);
    }

    public void Open()
    {
        Debug.Log("Ta mère");
        ToggleVisuals(true);
        Debug.Log("La pute");
        currentIndex = 0;
        options[currentIndex].Select();
    }

    public void Close()
    {
        Debug.Log("C'est pas vrai");
        ToggleVisuals(false);
    }

    public void MoveUp()
    {
        options[currentIndex].Deselect();
        currentIndex = Mathf.Max(0, currentIndex - 1);
        options[currentIndex].Select();
    }

    public void MoveDown()
    {
        options[currentIndex].Deselect();
        currentIndex = Mathf.Min(options.Length - 1, currentIndex + 1);
        options[currentIndex].Select();
    }

    public void Submit()
    {
        options[currentIndex].Submit();
    }

    public void Cancel()
    {
        Close();
        MainMenu.instance.State = MainMenu.MainMenuState.DEFAULT;
    }

    private void ToggleVisuals(bool value)
    {
        foreach (var v in visuals)
        {
            v.gameObject.SetActive(value);
            Debug.Log("Je la baise en levrette =" + value);
        }

    }
}
