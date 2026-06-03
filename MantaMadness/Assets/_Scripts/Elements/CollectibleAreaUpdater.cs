using UnityEngine;
using System.Collections;

public class CollectibleAreaUpdater : MonoBehaviour
{
    [SerializeField] private CollectibleArea collectibleAreaID;
    [SerializeField] private CollisionRelay relay;

    protected void Start()
    {
        StartCoroutine(DelayStart());
    }

    private IEnumerator DelayStart()
    {
        yield return null;
        yield return null;

        relay.HitCollision += UpdateCollectible;
    }

    private void OnDisable()
    {
        relay.HitCollision -= UpdateCollectible;
    }

    private void UpdateCollectible(SimpleController overload)
    {
        CollectibleAreaRegistry.Instance.SetCurrentArea(collectibleAreaID);
    }
}
