using UnityEngine;

public class CameraSilhouetteReplacement : MonoBehaviour
{
    public Material silhouetteMat;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnPreRender()
    {
        // Force all renderers on the Player layer to use the silhouette material
        cam.SetReplacementShader(silhouetteMat.shader, "");
    }

    void OnPostRender()
    {
        // Reset after rendering
        cam.ResetReplacementShader();
    }
}
