using UnityEngine;
using System.Collections.Generic;

public class AfterImagePool : MonoBehaviour
{
    public static AfterImagePool Instance;

    [Tooltip("Prefab doit contenir AfterImageGhost component.")]
    public GameObject ghostPrefab;
    public int poolSize = 20;
    public float ghostFadeSpeed = 10f;

    // pool list + track last used index for round-robin recycling
    private List<AfterImageGhost> pool = new List<AfterImageGhost>();
    private int nextRecycleIndex = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (ghostPrefab == null) { Debug.LogError("ghostPrefab manquant dans AfterImagePool"); return; }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(ghostPrefab, transform);
            var ghost = obj.GetComponent<AfterImageGhost>();
            if (ghost == null)
            {
                Debug.LogError("ghostPrefab doit posséder AfterImageGhost.");
                Destroy(obj);
                continue;
            }

            obj.SetActive(false);
            pool.Add(ghost);
        }
    }

    // Renvoie un ghost prêt à être utilisé.
    public AfterImageGhost GetGhost()
    {
        // 1) Chercher un ghost inactif (préférence forte)
        for (int i = 0; i < pool.Count; i++)
        {
            var g = pool[i];
            if (!g.gameObject.activeInHierarchy)
            {
                // found an inactive one -> ensure it's reset and return it
                g.ResetForReuse(); // safe-guard: make sure alpha & coroutines cleared
                return g;
            }
        }

        // 2) Aucun inactive trouvé -> recycler en round-robin
        // Choisis nextRecycleIndex, avance l'index pour la prochaine fois
        var recycled = pool[nextRecycleIndex];
        nextRecycleIndex = (nextRecycleIndex + 1) % pool.Count;

        // Force reset / cancel possible coroutines et remets l'alpha au max
        recycled.ResetForReuse();

        // IMPORTANT: si le ghost est encore actif (par ex. il n'a pas réussi à se désactiver),
        // on le force à désactiver maintenant afin d'éviter doublons visibles.
        if (recycled.gameObject.activeInHierarchy)
            recycled.gameObject.SetActive(false);

        return recycled;
    }

    // Optionnel : expose une méthode pour agrandir dynamiquement le pool
    public void ExpandPool(int additional)
    {
        for (int i = 0; i < additional; i++)
        {
            GameObject obj = Instantiate(ghostPrefab, transform);
            var ghost = obj.GetComponent<AfterImageGhost>();
            obj.SetActive(false);
            pool.Add(ghost);
        }
    }
}
