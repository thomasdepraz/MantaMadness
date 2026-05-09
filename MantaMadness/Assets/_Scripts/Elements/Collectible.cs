using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public enum CollectibleType { normal, super, mega, greyCoin, buckie }



[ExecuteInEditMode]
public class Collectible : MonoBehaviour, IDataPersistence
{
    private Coroutine routine;
    
    [SerializeField] private CollectibleType type;

    [SerializeField] private CollectibleRelay relay;

    private bool movingTowardtarget = false;
    private GameObject player;

    [SerializeField] private float speed = 0.8f;

    [Header("Save")]
    [SerializeField] private string collectibleID;

    private CollectibleAreaManager areaManager;

    [SerializeField]
    private CollectibleState collectibleState = CollectibleState.Active;

    public CollectibleState State => collectibleState;
    public string ID => collectibleID;

    public void LoadData(GameData data)
    {
        if (data.collectibleStates.TryGetValue(collectibleID, out CollectibleState savedState))
        {
            collectibleState = savedState;
        }

        ApplyState();
    }

    public int GetCollectibleValue()
    {
        switch (type)
        {
            case CollectibleType.normal:
                return 1;

            case CollectibleType.super:
                return 20;

            case CollectibleType.mega:
                return 100;

            case CollectibleType.buckie:
                return 1;

            case CollectibleType.greyCoin:
                return 100;
            default:
                return 0;
        }
    }

    public bool IsBuckie()
    {
        return type == CollectibleType.buckie;
    }

    public void SaveData(ref GameData data)
    {
        if (data.collectibleStates.ContainsKey(collectibleID))
        {
            data.collectibleStates[collectibleID] = collectibleState;
        }
        else
        {
            data.collectibleStates.Add(
                collectibleID,
                collectibleState
            );
        }
    }
#if UNITY_EDITOR
    [ContextMenu("Generate GUID")]
    private void GenerateGUID()
    {
        collectibleID = System.Guid.NewGuid().ToString();
    }
#endif

    private void OnEnable()
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

    private void OnDisable()
    {
        if (relay != null)
        {
            relay.HitCollision -= MoveToTarget;
        }
    }

    public void MoveToTarget(GameObject target)
    {
        if(movingTowardtarget == false)
        {
            player = target;
            movingTowardtarget = true;
        }
    }

    private void FixedUpdate()
    {
        if (movingTowardtarget == true && player != null)
        {
            transform.position = Vector3.Lerp(transform.position, player.transform.position, speed);
        }
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out SimpleController controller) && routine == null)
        {
            routine = StartCoroutine(PickupRoutine(controller));
        }
    }
    private void GiveReward()
    {
        int value = GetCollectibleValue();

        if (type == CollectibleType.buckie)
        {
            CoinManager.Instance.PickupBuckie(value);
            return;
        }

        if (type == CollectibleType.mega || type == CollectibleType.greyCoin)
        {
            UIManager.Instance.gameInterface.StartCoroutine("pickupMegaClam");
        }

        CoinManager.Instance.PickupClam(value);
    }

    private IEnumerator PickupRoutine(SimpleController controller)
    {
        //increase boost gauge
        //switch (type)
        //{
        //    case CollectibleType.normal:
        //        //Increase coincount
        //        CoinManager.Instance.PickupClam(1);
        //        break;
        //    case CollectibleType.super:
        //        //Increase coincount
        //        CoinManager.Instance.PickupClam(20);
        //        break;
        //    case CollectibleType.mega:
        //        UIManager.Instance.gameInterface.StartCoroutine("pickupMegaClam");
        //        //Increase coincount
        //        CoinManager.Instance.PickupClam(100);
        //        break;
        //    case CollectibleType.greyCoin:
        //        UIManager.Instance.gameInterface.StartCoroutine("pickupMegaClam");
        //        //Increase coincount
        //        CoinManager.Instance.PickupClam(100);
        //        break;
        //    case CollectibleType.buckie:
        //        //UIManager.Instance.gameInterface.StartCoroutine("pickupBuckie");
        //        CoinManager.Instance.PickupBuckie(1);
        //        break;
        //    default:
        //        break;
        //}

        GiveReward();

        //Play particle explosion
        MantaVisuals.instance.PickupParticles();

        collectibleState = CollectibleState.Inactivable;

        if (UIManager.Instance != null && UIManager.Instance.gameInterface != null)
        {
            UIManager.Instance.gameInterface.RefreshAreaClamCount();
            UIManager.Instance.gameInterface.RefreshAreaBuckieCount();
        }

        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.SaveGame();
        }

        //deactivate tween
        Sequence tween = DOTween.Sequence().Append(transform.DOScale(0, 0.1f).SetEase(Ease.OutBounce));
        tween.Append(transform.DOJump(transform.position + transform.up, 1, 1, 0.8f));
        
        tween.onComplete += ()=> gameObject.SetActive(false);

        yield return null;
    }

    private void ApplyState()
    {
        switch (collectibleState)
        {
            case CollectibleState.Active:
                gameObject.SetActive(true);
                break;

            case CollectibleState.Activable:
                gameObject.SetActive(false);
                break;

            case CollectibleState.Inactivable:
                gameObject.SetActive(false);
                break;
        }
    }

    public void ActivateCollectible()
    {
        if (collectibleState == CollectibleState.Activable)
        {
            collectibleState = CollectibleState.Active;
            gameObject.SetActive(true);
        }
    }


}
