
using System.Collections;
using DG.Tweening;
using UnityEngine;
using FMODUnity;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider))]
public class HelixItem : MonoBehaviour
{
    [Header("Identification")]
    [SerializeField, Min(0)] private int id;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Objects")]
    [Tooltip("Objets normaux activés par cette Helix.")]
    [SerializeField] private GameObject[] linkedObjects;

    [Tooltip("Collectibles activés par cette Helix.")]
    [SerializeField] private Collectible[] collectibleRewards;

    [Header("Spawn Timing")]
    [Tooltip("Durée totale approximative de l'apparition des objets.")]
    [SerializeField, Min(0f)] private float spawnTime = 2f;

    [Header("Spawn Animation")]
    [SerializeField] private float verticalMovement = 5f;
    [SerializeField, Min(0.01f)] private float movementDuration = 0.2f;
    [SerializeField] private Ease movementEase = Ease.OutQuad;

    [Header("Helix References")]
    [Tooltip("Visuel de la Helix. Peut être un enfant de cet objet.")]
    [SerializeField] private GameObject helixVisual;
    [SerializeField] private ParticleSystem sparkle;
    [SerializeField] private ParticleSystem fogParticle;
    [SerializeField] private ParticleSystem sparkleExplosion;
    [SerializeField] private ParticleSystem sparkleExplosionEnd;

    [SerializeField] private Collider triggerCollider;

    [Header("Completed State")]
    [Tooltip("Objets à désactiver lorsque cette Helix est terminée.")]
    [SerializeField] private GameObject[] toDeactivateOnComplete;

    [Tooltip("Objets à activer lorsque cette Helix est terminée.")]
    [SerializeField] private GameObject[] toActivateOnComplete;

    [Header("Cinematic")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private CinemachineBlendDefinition blend;
    [SerializeField] private EventReference activationSound;
    [SerializeField, Min(0f)] private float cameraStartDelay = 1f;
    [SerializeField, Min(0f)] private float cameraEndDelay = 0.8f;

    public CinemachineCamera Vcam => vcam;
    public CinemachineBlendDefinition Blend => blend;
    public EventReference ActivationSound => activationSound;
    public float CameraStartDelay => cameraStartDelay;
    public float CameraEndDelay => cameraEndDelay;

    private HelixManager manager;

    private bool isAvailable;
    private bool isCompleted;
    private bool isInitialized;

    public int ID => id;
    public bool IsAvailable => isAvailable && !isCompleted;
    public bool IsCompleted => isCompleted;

    private void Reset()
    {
        triggerCollider = GetComponent<Collider>();

        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void Awake()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }
    }

    public void Initialize(HelixManager helixManager)
    {
        manager = helixManager;
        isInitialized = true;

        PrepareInitialState();
    }

    private void PrepareInitialState()
    {
        if (linkedObjects != null)
        {
            foreach (GameObject linkedObject in linkedObjects)
            {
                if (linkedObject != null)
                {
                    linkedObject.SetActive(false);
                }
            }
        }

        if (toActivateOnComplete != null)
        {
            foreach (GameObject target in toActivateOnComplete)
            {
                if (target != null)
                {
                    target.SetActive(false);
                }
            }
        }

        if (toDeactivateOnComplete != null)
        {
            foreach (GameObject target in toDeactivateOnComplete)
            {
                if (target != null)
                {
                    target.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            $"CONTACT HELIX {id} avec {other.name} | " +
            $"initialized={isInitialized} | " +
            $"available={IsAvailable} | " +
            $"tag={other.tag}",
            this
        );

        if (!isInitialized)
            return;

        if (!IsAvailable)
            return;

        if (!other.CompareTag(playerTag))
            return;

        TriggerHelix();
    }

    public void TriggerHelix()
    {
        if (!IsAvailable)
            return;

        if (manager == null)
        {
            Debug.LogError(
                $"[{nameof(HelixItem)}] La Helix {id} n'a aucun manager.",
                this
            );

            return;
        }

        manager.TryStartHelix(this);
    }

    public void DisableInteraction()
    {
        isAvailable = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }
    }

    public void DisableAnimation()
    {
        StartCoroutine(DisableAnimationSequence());
    }

    public IEnumerator DisableAnimationSequence()
    {
        Transform t = helixVisual.transform;

        t.DOKill();

        Sequence sequence = DOTween.Sequence();

        sparkleExplosion.Play();
        sparkle.Stop();
        fogParticle.Stop();

        // Spin très rapide pendant toute l'animation
        sequence.Join(
            t.DORotate(
                new Vector3(0f, 2070f, 0f),
                1.1f,
                RotateMode.FastBeyond360
            ).SetEase(Ease.Linear)
        );

        // Petit plongeon
        sequence.Join(
            t.DOMoveY(
                t.position.y - 0.75f,
                1.1f
            ).SetEase(Ease.InQuad)
        );

        // Sparkle ici
        // sparkleBurst.Play();

        // Petite pause
        sequence.AppendInterval(0.05f);

        // Remonte en accélérant
        sequence.Append(
            t.DOMoveY(
                t.position.y + 2.5f,
                0.5f
            ).SetEase(Ease.OutExpo)
        );

        // Rétrécit pendant la remontée
        sequence.Join(
            t.DOScale(
                Vector3.zero,
                0.5f
            ).SetEase(Ease.InBack)
        );

        sequence.Join(
        t.DORotate(
            new Vector3(0f, 2070f, 0f),
            0.5f,
            RotateMode.FastBeyond360
        ).SetEase(Ease.Linear)
    );

        yield return sequence.WaitForCompletion();

        helixVisual.SetActive(false);
    }

    public void SetHelixActive(bool active)
    {
        if (isCompleted)
        {
            active = false;
        }

        isAvailable = active;

        if (helixVisual != null)
        {
            helixVisual.SetActive(active);
        }

        if (triggerCollider != null)
        {
            triggerCollider.enabled = active;
        }

        if(IsAvailable == true)
        {
            sparkle.Play();
        }

        Debug.Log(
            $"[HELIX ITEM {id}] Active: {active} | " +
            $"Available: {isAvailable} | " +
            $"Collider: {(triggerCollider != null && triggerCollider.enabled)}",
            this
        );
    }

    /// <summary>
    /// Apparition animée de cette Helix.
    /// Utilisé pour la prochaine Helix dans la séquence.
    /// </summary>
    public IEnumerator SpawnHelixRoutine()
    {
        isCompleted = false;
        isAvailable = false;

        if (helixVisual != null)
        {
            helixVisual.SetActive(true);
            PlaySpawnAnimation(helixVisual.transform);
        }

        if (movementDuration > 0f)
        {
            yield return new WaitForSeconds(movementDuration * 2f);
        }

        isAvailable = true;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }

    /// <summary>
    /// Fait apparaître tous les objets liés à cette Helix.
    /// </summary>
    public IEnumerator SpawnLinkedObjectsRoutine()
    {
        int validSpawnCount = CountValidSpawnObjects();

        if (validSpawnCount <= 0)
            yield break;

        float delayBetweenSpawns = spawnTime / validSpawnCount;

        if (linkedObjects != null)
        {
            foreach (GameObject linkedObject in linkedObjects)
            {
                if (linkedObject == null)
                    continue;

                linkedObject.SetActive(true);
                PlaySpawnAnimation(linkedObject.transform);

                if (delayBetweenSpawns > 0f)
                {
                    yield return new WaitForSeconds(delayBetweenSpawns);
                }
            }
        }

        if (collectibleRewards != null)
        {
            foreach (Collectible collectible in collectibleRewards)
            {
                if (collectible == null)
                    continue;

                if (collectible.State != CollectibleState.Activable)
                    continue;

                collectible.ActivateCollectible();
                PlaySpawnAnimation(collectible.transform);

                if (delayBetweenSpawns > 0f)
                {
                    yield return new WaitForSeconds(delayBetweenSpawns);
                }
            }
        }
    }

    private int CountValidSpawnObjects()
    {
        int count = 0;

        if (linkedObjects != null)
        {
            foreach (GameObject linkedObject in linkedObjects)
            {
                if (linkedObject != null)
                {
                    count++;
                }
            }
        }

        if (collectibleRewards != null)
        {
            foreach (Collectible collectible in collectibleRewards)
            {
                if (collectible != null &&
                    collectible.State == CollectibleState.Activable)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private void PlaySpawnAnimation(Transform target)
    {
        if (target == null)
            return;

        target.DOKill();

        target.DOMoveY(
                target.position.y + verticalMovement,
                movementDuration
            )
            .SetEase(movementEase)
            .SetLoops(2, LoopType.Yoyo);
    }

    public void MarkCompleted()
    {
        isCompleted = true;
        isAvailable = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (helixVisual != null)
        {
            helixVisual.SetActive(false);
        }

        ApplyCompletedObjects();
    }

    /// <summary>
    /// Restaure une Helix terminée sans animation.
    /// Utilisé lors du chargement.
    /// </summary>
    public void RestoreCompletedState()
    {
        isCompleted = true;
        isAvailable = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (helixVisual != null)
        {
            helixVisual.SetActive(false);
        }

        RestoreLinkedObjects();
        ApplyCompletedObjects();
    }

    public void SetCompletedState(bool completed)
    {
        isCompleted = completed;
        isAvailable = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        if (!completed)
        {
            ResetCompletedObjects();
        }
    }

    private void RestoreLinkedObjects()
    {
        if (linkedObjects != null)
        {
            foreach (GameObject linkedObject in linkedObjects)
            {
                if (linkedObject != null)
                {
                    linkedObject.SetActive(true);
                }
            }
        }

        /*
         * Les collectibles restaurent normalement leur propre état
         * via leur système IDataPersistence.
         */
    }

    private void ApplyCompletedObjects()
    {
        if (toDeactivateOnComplete != null)
        {
            foreach (GameObject target in toDeactivateOnComplete)
            {
                if (target != null)
                {
                    target.SetActive(false);
                }
            }
        }

        if (toActivateOnComplete != null)
        {
            foreach (GameObject target in toActivateOnComplete)
            {
                if (target != null)
                {
                    target.SetActive(true);
                }
            }
        }
    }

    private void ResetCompletedObjects()
    {
        if (toDeactivateOnComplete != null)
        {
            foreach (GameObject target in toDeactivateOnComplete)
            {
                if (target != null)
                {
                    target.SetActive(true);
                }
            }
        }

        if (toActivateOnComplete != null)
        {
            foreach (GameObject target in toActivateOnComplete)
            {
                if (target != null)
                {
                    target.SetActive(false);
                }
            }
        }
    }
}

