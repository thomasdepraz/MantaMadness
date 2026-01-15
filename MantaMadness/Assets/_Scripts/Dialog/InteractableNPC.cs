using UnityEngine;

public class InteractableNPC : MonoBehaviour
{
    [SerializeField] private GameObject outOfRangeVisual;
    [SerializeField] private GameObject inRangeVisual;
    [SerializeField] public string dialogKey;

    private void Start()
    {
        if(inRangeVisual.activeSelf == true)
        {
            DisableVisual();
        }
    }

    public void EnableVisual()
    {
        outOfRangeVisual.SetActive(false);
        inRangeVisual.SetActive(true);
    }

    public void DisableVisual()
    {
        outOfRangeVisual.SetActive(true);
        inRangeVisual.SetActive(false);
    }
}
