using UnityEngine;

public class InteractableNPC : MonoBehaviour
{
    [SerializeField] private GameObject interactionVisual;

    [SerializeField] public string dialogKey;

    private void Start()
    {
        if(interactionVisual.activeSelf == true)
        {
            DisableVisual();
        }
    }

    public void EnableVisual()
    {
        interactionVisual.SetActive(true);
    }

    public void DisableVisual()
    {
        interactionVisual.SetActive(false);
    }
}
