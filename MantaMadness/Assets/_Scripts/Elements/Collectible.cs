using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private Coroutine routine;
    
    [SerializeField] private CollectibleType type;
    public enum CollectibleType { normal, super, mega}

    [SerializeField] private CollectibleRelay relay;

    private bool movingTowardtarget = false;
    private GameObject player;


    private void Start()
    {
        if(relay != null)
        {
            relay.HitCollision += MoveToTarget;
        }
        else
        {
            print("Careful! this clam doesn't have a CollectibleRelay!");
        }

    }

    private void MoveToTarget(GameObject target)
    {
        player = target;
        movingTowardtarget = true;
    }


    private void FixedUpdate()
    {
        if (movingTowardtarget == true && player != null)
        {
            transform.position = Vector3.Lerp(transform.position, player.transform.position, 0.4f);
        }
    }
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
        //SoundManager.Instance.PlayOneShotSound(SoundType.COINPICKUP);

        //increase coin count
        CoinManager.Instance.PickupCollectible();

        //increase boost gauge
        switch (type)
        {
            case CollectibleType.normal:
                controller.boostBehaviour.IncrementGauge(BoostAction.Collectible);
                break;
            case CollectibleType.super:
                controller.boostBehaviour.IncrementGauge(BoostAction.SuperCollectible);
                break;
            case CollectibleType.mega:
                controller.boostBehaviour.IncrementGauge(BoostAction.MegaCollectible);
                UIManager.Instance.gameInterface.StartCoroutine("pickupMegaClam");
                break;
            default:
                controller.boostBehaviour.IncrementGauge(BoostAction.Collectible);
                break;
        }


        //deactivate twwen

        Sequence tween = DOTween.Sequence().Append(transform.DOScale(0, 0.1f).SetEase(Ease.OutBounce));
        tween.Append(transform.DOJump(transform.position + transform.up, 1, 1, 0.8f));
        
        tween.onComplete += ()=>gameObject.SetActive(false);

        yield return null;
    }
}
