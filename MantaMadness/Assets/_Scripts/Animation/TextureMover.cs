using UnityEngine;

public class TextureMover : MonoBehaviour
{
    public GameObject model;
    private Material texture;

    private void Start()
    {
        SkinnedMeshRenderer mesh = model.GetComponent<SkinnedMeshRenderer>();
        texture = mesh.material;
    }

    public void UpdateOffset(float offset)
    {
        texture.SetVector("_Texture_Offset", new Vector2(offset,0));
    }
}
