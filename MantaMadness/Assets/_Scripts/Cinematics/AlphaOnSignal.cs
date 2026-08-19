using UnityEngine;
using DG.Tweening;
public class AlphaOnSignal : MonoBehaviour
{
    [SerializeField] private MeshRenderer mesh;
    [SerializeField] private float duration = 0.5f;

    private static readonly int AlphaID = Shader.PropertyToID("_Alpha");

    private Tween alphaTween;

    private void OnEnable()
    {
        mesh.material.SetFloat(AlphaID, 0);
    }

    public void OnSignalEnable()
    {
        if (mesh == null) return;

        // Évite que deux tweens se battent sur la même propriété
        alphaTween?.Kill();

        alphaTween =  mesh.material
            .DOFloat(1f, AlphaID, duration)
            .SetEase(Ease.InOutSine);
    }

    public void OnSignalDisable()
    {
        if (mesh == null) return;

        alphaTween?.Kill();

        alphaTween = mesh.material
            .DOFloat(0f, AlphaID, duration)
            .SetEase(Ease.InOutSine);
    }

    private void OnDestroy()
    {
        alphaTween?.Kill();
    }
}
