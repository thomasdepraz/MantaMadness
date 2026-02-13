using UnityEngine;
using static OptionsMenu;

public class Credits : MonoBehaviour
{
    public static Credits instance;

    [Header("All visuals")]
    [SerializeField] private GameObject[] visuals;
    private bool blockSubmit;

    [Header("Navigation")]
    [SerializeField] public CreditsBack creditItems;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        ToggleVisuals(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);

        Enable();



        blockSubmit = true;
        StartCoroutine(UnblockSubmitNextFrame());
    }

    private System.Collections.IEnumerator UnblockSubmitNextFrame()
    {
        yield return null;
        blockSubmit = false;
    }

    public void Enable()
    {
        ToggleVisuals(true);
    }

    public void ToggleVisuals(bool toggleValue)
    {
        if (toggleValue)
        {
            foreach (GameObject visual in visuals)
            {
                visual.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject visual in visuals)
            {
                visual.SetActive(false);
            }
        }
    }

    public void Submit()
    {
        if (blockSubmit) return;
        creditItems.Submit();
    }

    public void Close()
    {
        ToggleVisuals(false);
        gameObject.SetActive(false);

        MainMenu.instance.State = MainMenu.MainMenuState.DEFAULT;
    }
}
