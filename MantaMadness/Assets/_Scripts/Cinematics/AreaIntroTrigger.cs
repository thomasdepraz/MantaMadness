using UnityEngine;

public class AreaIntroTrigger : MonoBehaviour
{
    [SerializeField] private AreaIntro areaIntro;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out SimpleController controller))
        {
            areaIntro.Play();
        }
    }
}
