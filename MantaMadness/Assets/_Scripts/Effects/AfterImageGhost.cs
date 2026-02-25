using UnityEngine;
using System.Collections;
using DG.Tweening;

public class AfterImageGhost : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private Material matInstance;
    private Color baseColor;
    private Color endColor;
    private float fadeDuration;

    public Coroutine fadeCoroutine;
    private Tween colorTween;

    void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    public void Initialize(Material mat, float fade)
    {
        // Stop tween/coroutine from previous use
        KillTweensAndCoroutines();

        // Create or refresh the runtime material instance
        if (matInstance == null)
            matInstance = new Material(mat);
        else
            matInstance.CopyPropertiesFromMaterial(mat); // <-- CRUCIAL

        // Assign the refreshed material instance
        // sharedMaterial avoids Unity silently creating extra instances
        meshRenderer.sharedMaterial = matInstance;

        // Recompute base/end colors for THIS material
        baseColor = matInstance.color;
        endColor = baseColor;
        endColor.a = 0f;

        fadeDuration = fade;

        // Ensure starts fully visible (in case recycled mid-fade)
        matInstance.color = baseColor;
    }

    public void SetMesh(Mesh mesh)
    {
        meshFilter.sharedMesh = mesh;
    }

    public void Show(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        transform.SetPositionAndRotation(pos, rot);
        transform.localScale = scale;

        gameObject.SetActive(true);

        KillTweensAndCoroutines();

        fadeCoroutine = StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {
        // Make sure we start from baseColor every time
        matInstance.color = baseColor;

        colorTween = matInstance
            .DOColor(endColor, fadeDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true); // optionnel: ignore timeScale si tu veux

        yield return new WaitForSeconds(fadeDuration);

        gameObject.SetActive(false);
        fadeCoroutine = null;
    }

    public void ResetForReuse()
    {
        KillTweensAndCoroutines();

        if (matInstance != null)
            matInstance.color = baseColor;

        gameObject.SetActive(false);
    }

    private void KillTweensAndCoroutines()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (colorTween != null && colorTween.IsActive())
        {
            colorTween.Kill();
            colorTween = null;
        }

        StopAllCoroutines(); // si tu veux être ultra safe (optionnel si tu gères bien fadeCoroutine)
    }
}