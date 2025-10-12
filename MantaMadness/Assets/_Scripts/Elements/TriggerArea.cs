using UnityEngine;

[RequireComponent (typeof(Collider))]
public class TriggerArea : MonoBehaviour
{
    [SerializeField] private GameObject[] toActivate;

    private void Start()
    {
        foreach(GameObject element in toActivate)
        {
            if (element.activeSelf == true)
            {
                element.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            ActivateObjects();
        }
    }

    private void ActivateObjects()
    {
        if (toActivate.Length > 0)
        {
            foreach(GameObject element in toActivate)
            {
                if(element.gameObject.activeSelf == false)
                {
                    element.SetActive(true);
                }
            }
        }
    }
}
