using DG.Tweening;
using UnityEngine;
using System.Collections;

public class DefaultButtonActivation : MonoBehaviour, IButtonAction
{
    [SerializeField] private UnityEngine.GameObject[] objectsToActivate;
    [SerializeField] private UnityEngine.GameObject[] objectsToDeactivate;
    [SerializeField] private float spawnTime = 0.05f;

    public void Execute()
    {
        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        foreach (UnityEngine.GameObject obj in objectsToDeactivate)
        {
            obj.SetActive(false);
        }

        if (objectsToActivate.Length > 0)
        {
            for (int i = 0; i < objectsToActivate.Length; i++)
            {
                UnityEngine.GameObject obj = objectsToActivate[i];

                obj.SetActive(true);

                obj.transform.DOMoveY(obj.transform.position.y + 5f, 0.2f)
                    .SetEase(Ease.OutQuad)
                    .SetLoops(2, LoopType.Yoyo);

                yield return new WaitForSeconds(spawnTime / objectsToActivate.Length);
            }
        }
    }
}
