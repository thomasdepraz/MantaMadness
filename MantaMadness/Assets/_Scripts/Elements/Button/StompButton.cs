using UnityEngine;
using DG.Tweening;

public class StompButon : Button
{

    [SerializeField] private GameObject buttonVisual;
    [SerializeField] private Collider buttonVisualCollision;
    [SerializeField] private Transform buttonOriginalPos;
    [SerializeField] private Transform buttonActivatedPos;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if (controller.State == ControllerState.STOMP)
            {
                controller.Bump(new Vector3(0, 1, 1));
                if (isActivated == false && routine == null)
                {
                    ButtonImpactEffect();
                    routine = StartCoroutine(ActivationCoroutine(controller));
                }
            }
        }
    }

    private void ButtonImpactEffect()
    {
        buttonVisual.transform.DOMove(buttonActivatedPos.position,0.2f).SetEase(Ease.InOutQuad);
        buttonVisualCollision.enabled = false;
    }
}
