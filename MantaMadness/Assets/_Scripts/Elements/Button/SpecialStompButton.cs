using DG.Tweening;
using FMODUnity;
using System.Collections;
using UnityEngine;

public class SpecialStompButton : StompButton
{

    protected SpecialStompButtonManager manager;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller))
        {
            if (controller.State == ControllerState.STOMP || controller.State == ControllerState.ANTIGRAVJUMP)
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

    protected override void ButtonImpactEffect()
    {
        buttonVisual.transform.DOMove(buttonActivatedPos.position, 0.2f).SetEase(Ease.InOutQuad);
        buttonVisualCollision.enabled = false;
    }
    public void SetManager(SpecialStompButtonManager newManager)
    {
        manager = newManager;
    }

    protected override IEnumerator ActivationCoroutine(SimpleController controller)
    {
        manager.RegisterDestruction(this);
        isActivated = true;
        buttonMesh.material = activatedMaterial;

        RuntimeManager.PlayOneShot(buttonStinger, Camera.main.transform.position);
        yield return null;
    }
}
