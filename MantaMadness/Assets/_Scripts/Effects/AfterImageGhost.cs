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

    //private bool fading;

    void Awake()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    public void Initialize(Material mat, float fade)
    {
        if (matInstance == null)
            matInstance = new Material(mat);

        meshRenderer.material = matInstance;
        baseColor = matInstance.color;
        endColor = baseColor;
        endColor.a = 0f;
        fadeDuration = fade;
        //fading = false;
    }

    public void SetMesh(Mesh mesh)
    {
        meshFilter.mesh = mesh;
    }

    public void Show(Vector3 pos, Quaternion rot, Vector3 scale)
    {
        transform.SetPositionAndRotation(pos, rot);
        transform.localScale = scale;
        gameObject.SetActive(true);
        StopAllCoroutines();
        fadeCoroutine = StartCoroutine(FadeRoutine());
    }
    public Coroutine fadeCoroutine;
    IEnumerator FadeRoutine()
    {

        matInstance.DOColor(endColor, fadeDuration).SetEase(Ease.OutQuad);

        // IMPORTANT: désactive ici pour signaler au pool qu'il est réutilisable
        yield return new WaitForSeconds(fadeDuration);
        gameObject.SetActive(false);
        fadeCoroutine = null;
        yield return null;
    }
    public void ResetForReuse()
    {
        // stop any fading coroutine
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        // reset alpha to original
        if (matInstance != null)
        {
            Color c = matInstance.color;
            c.a = baseColor.a;
            matInstance.color = c;
        }

        // ensure it's inactive so pool can hand it out next time
        gameObject.SetActive(false);
    }
}
