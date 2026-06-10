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
        ToggleVisuals(true);
        currentIndex = 0;

        for(int i = 0; i < options.Length; i++)
        {
            if(i == currentIndex)
            {
                options[i].Select();
            }
            else
            {
                options[i].Deselect();
            }
        }
    }

    public void Close()
    {
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
        }
    }
}
