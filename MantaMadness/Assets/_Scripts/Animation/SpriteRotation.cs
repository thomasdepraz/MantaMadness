using UnityEngine;

public class SpriteRotation : MonoBehaviour
{
    private Camera m_Camera;

    void Start()
    {
        m_Camera = Camera.main;
    }

    void Update()
    {
        gameObject.transform.LookAt(m_Camera.transform);
    }
}
