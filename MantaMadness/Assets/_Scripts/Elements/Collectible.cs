using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private Coroutine routine;
    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller) && routine == null)
        {
            routine = StartCoroutine(PickupRoutine(controller));
        }
    }

    private IEnumerator PickupRoutine(SimpleController controller)
    {
        //sound
        SoundManager.Instance.PlayOneShotSound(SoundType.COINPICKUP);

        //increase coin count
        CoinManager.Instance.PickupCollectible();

        //increase boost gauge
        controller.boostBehaviour.IncrementGauge(BoostAction.Collectible);

        //deactivate twwen

        Sequence tween = DOTween.Sequence().Append(transform.DOScale(0, 1).SetEase(Ease.OutBounce));
        tween.Append(transform.DOJump(transform.position + transform.up, 1, 1, 0.8f));
        
        tween.onComplete += ()=>gameObject.SetActive(false);

        yield return null;
    }
}
