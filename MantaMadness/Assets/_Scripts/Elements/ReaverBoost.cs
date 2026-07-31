using UnityEngine;

public class ReaverBoost : MonoBehaviour
{
    //Scripts for the Reaver boost level elements.
    //Similar to rails, player will get one as a reference and enter a special phase upon touching one

    [SerializeField] private BoxCollider boostCollider;
    [SerializeField] private float exitMargin = 0.1f;

    private void Awake()
    {
        if (boostCollider == null)
            boostCollider = GetComponent<BoxCollider>();
    }

    public bool HasPassedTop(Vector3 playerWorldPosition)
    {
        Vector3 localPosition =
            boostCollider.transform.InverseTransformPoint(playerWorldPosition);

        float top =
            boostCollider.center.y +
            boostCollider.size.y * 0.5f;

        return localPosition.y > top + exitMargin;
    }
}
