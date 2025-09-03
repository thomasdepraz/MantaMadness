using UnityEngine;

public class BubbleCanon : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            controller.inBubbleCanon = true;
        }
    }
}
