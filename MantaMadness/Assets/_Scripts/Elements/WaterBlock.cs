using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class WaterBlock : MonoBehaviour
{
    private BoxCollider boxCollider;

    float bottomHeight;
    float topHeight;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider>();
        bottomHeight = boxCollider.bounds.min.y;
        topHeight = boxCollider.bounds.max.y;
    }

    public float GetDepthAtPosition(Vector3 position, out bool isOut)
    {
        isOut = false;
        float depth = topHeight - position.y;
        if(topHeight - depth < bottomHeight)
        {
            depth = 0;
            isOut = false;
        }

        return depth;
    }

}
