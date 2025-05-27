using UnityEngine;
using DG.Tweening;

public class TransitionInterface : MonoBehaviour, IScreen
{
    public GameObject Container { get => container; }
    public GameObject container;

    public RectTransform fadeTransform;

    private void Start()
    {
        UIManager.Instance.transitionScreen = this;
    }

    public void TransitionIn()
    {
        fadeTransform.DOScale(new Vector3(30, 30, 1), 0.6f).SetEase(Ease.OutQuad).OnComplete(() => TransitionOut());
    }

    public void TransitionOut()
    {
        fadeTransform.DOScale(Vector3.zero, 0.6f).SetEase(Ease.InQuad);
    }
}
