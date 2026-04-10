using UnityEngine;
using DG.Tweening;
using System.Collections;

public class TransitionInterface : MonoBehaviour, IScreen
{
    public GameObject Container { get => container; }
    public GameObject container;

    public RectTransform fadeTransform;

    private void Start()
    {
        UIManager.Instance.transitionScreen = this;
        TransitionOnLoad();
    }

    public void TransitionOnLoad()
    {
        fadeTransform.localScale = new Vector3(30, 30, 1);
    }

    public void TransitionInOut()
    {
        fadeTransform.DOScale(new Vector3(30, 30, 1), 0.6f).SetEase(Ease.OutQuad).OnComplete(() => TransitionOut());
    }

    public void TransitionIn()
    {
        fadeTransform.DOScale(new Vector3(30, 30, 1), 0.6f).SetEase(Ease.OutQuad);
    }

    public void TransitionOut()
    {
        fadeTransform.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InQuad);
    }
}
