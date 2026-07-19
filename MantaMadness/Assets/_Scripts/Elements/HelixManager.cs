using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;

public class HelixManager : MonoBehaviour, IDataPersistence
{
    [Header("Save")]
    [Tooltip("Identifiant unique de cette séquence d'Helix.")]
    [SerializeField] private string moduleID;

    [Header("Helix Sequence")]
    [SerializeField] private HelixItem[] helixItems;

    [Header("Startup")]
    [Tooltip("ID de la première Helix active lorsqu'aucune sauvegarde n'existe.")]
    [SerializeField] private int startingHelixID = 0;

    [Header("Events")]
    public UnityEvent onHelixStarted;
    public UnityEvent onHelixCompleted;
    public UnityEvent onSequenceCompleted;

    private readonly Dictionary<int, HelixItem> helixByID = new();

    private bool sequenceRunning;
    private bool sequenceInitialized;

    /*
     * ID de la Helix actuellement active.
     *
     * Lorsque toute la séquence est terminée :
     * currentSequenceID = helixItems.Length + 1
     */
    private int currentSequenceID;

    private int FinishedSequenceValue => helixItems.Length;

    private void Awake()
    {
        if (!sequenceInitialized)
        {
            InitializeHelixSequence();
        }
    }

    private void InitializeHelixSequence()
    {
        if (sequenceInitialized)
            return;

        helixByID.Clear();

        if (helixItems == null || helixItems.Length == 0)
        {

            sequenceInitialized = false;
            return;
        }

        helixItems = helixItems
            .Where(helix => helix != null)
            .OrderBy(helix => helix.ID)
            .ToArray();

        foreach (HelixItem helix in helixItems)
        {
            if (helixByID.ContainsKey(helix.ID))
            {
                continue;
            }

            helixByID.Add(helix.ID, helix);

            helix.Initialize(this);
            helix.SetCompletedState(false);
            helix.SetHelixActive(false);
        }

        currentSequenceID = startingHelixID;
        sequenceInitialized = true;

    }

    public void LoadData(GameData data)
    {

        if (!sequenceInitialized)
        {
            InitializeHelixSequence();
        }

        if (!sequenceInitialized)
            return;

        if (data == null)
        {
            return;
        }

        if (data.helixSequences == null)
        {
            ApplySequenceState(startingHelixID);
            return;
        }

        bool found = data.helixSequences.TryGetValue(
            moduleID,
            out int savedSequenceID
        );

        ApplySequenceState(
            found
                ? savedSequenceID
                : startingHelixID
        );
    }

    public void SaveData(ref GameData data)
    {
        if (string.IsNullOrWhiteSpace(moduleID))
        {
            return;
        }

        if (data.helixSequences.ContainsKey(moduleID))
        {
            data.helixSequences[moduleID] = currentSequenceID;
        }
        else
        {
            data.helixSequences.Add(moduleID, currentSequenceID);
        }
    }

    private void ApplySequenceState(int loadedSequenceID)
    {
        sequenceRunning = false;

        // Désactive tout avant d'appliquer la sauvegarde.
        foreach (HelixItem helix in helixItems)
        {
            if (helix == null)
                continue;

            helix.SetCompletedState(false);
            helix.SetHelixActive(false);
        }

        // ID supérieur au nombre d'Helix = module terminé.
        if (loadedSequenceID >= FinishedSequenceValue)
        {
            currentSequenceID = FinishedSequenceValue;

            foreach (HelixItem helix in helixItems)
            {
                if (helix != null)
                {
                    helix.RestoreCompletedState();
                }
            }

            Debug.Log("[HELIX LOAD] Module déjà terminé.", this);
            return;
        }

        // Active uniquement la Helix correspondant à la sauvegarde.
        if (helixByID.TryGetValue(
            loadedSequenceID,
            out HelixItem activeHelix
        ))
        {
            currentSequenceID = loadedSequenceID;

            foreach (HelixItem helix in helixItems)
            {
                if (helix == null)
                    continue;

                if (helix.ID < loadedSequenceID)
                {
                    helix.RestoreCompletedState();
                }
                else
                {
                    helix.SetCompletedState(false);
                    helix.SetHelixActive(helix == activeHelix);
                }
            }

            return;
        }
    }

    /// <summary>
    /// Appelé par une Helix lorsque le joueur entre dans son trigger.
    /// </summary>
    public bool TryStartHelix(HelixItem helix)
    {
        if (helix == null)
            return false;

        if (sequenceRunning)
        {
            return false;
        }

        if (!helix.IsAvailable)
        {
            return false;
        }

        if (helix.ID != currentSequenceID)
        {
            return false;
        }

        StartCoroutine(HelixSequenceRoutine(helix));
        return true;
    }

    private IEnumerator HelixSequenceRoutine(HelixItem currentHelix)
    {
        sequenceRunning = true;

        currentHelix.DisableInteraction();

        currentHelix.DisableAnimation();


        yield return currentHelix.DisableAnimationSequence();

        Game.Instance.player.ForceLock(true);
        Game.Instance.player.RailLock(true);

        CinemachineCamera currentCamera = currentHelix.Vcam;

        if (currentCamera != null)
        {
            currentCamera.enabled = true;

            CameraManager.Instance.BlendToCamera(
                currentCamera,
                currentHelix.Blend
            );
        }

        if (!currentHelix.ActivationSound.IsNull)
        {
            Vector3 soundPosition = currentCamera != null
                ? currentCamera.transform.position
                : currentHelix.transform.position;

            RuntimeManager.PlayOneShot(
                currentHelix.ActivationSound,
                soundPosition
            );
        }

        if (currentHelix.CameraStartDelay > 0f)
        {
            yield return new WaitForSeconds(
                currentHelix.CameraStartDelay
            );
        }

        UIEffectManager.Instance.GoodAction.Invoke();

        yield return currentHelix.SpawnLinkedObjectsRoutine();

        currentHelix.MarkCompleted();

        HelixItem nextHelix = FindNextHelix(currentHelix.ID);

        if (nextHelix != null)
        {
            currentSequenceID = nextHelix.ID;
        }
        else
        {
            currentSequenceID = FinishedSequenceValue;
        }

        DataPersistenceManager.Instance.SaveGame();

        if (currentHelix.CameraEndDelay > 0f)
        {
            yield return new WaitForSeconds(currentHelix.CameraEndDelay);
        }

        if (currentHelix.Vcam != null)
        {
            CameraManager.Instance.ResetCamera(currentHelix.Vcam);
        }

        Game.Instance.player.ForceLock(false);
        Game.Instance.player.RailLock(false);

        // D'abord autoriser une nouvelle séquence.
        sequenceRunning = false;

        // Ensuite seulement activer la prochaine Helix.
        if (nextHelix != null)
        {
            nextHelix.SetCompletedState(false);
            nextHelix.SetHelixActive(true);
        }
    }

    private HelixItem FindNextHelix(int currentID)
    {
        if (helixByID.TryGetValue(
            currentID + 1,
            out HelixItem directNext
        ))
        {
            if (!directNext.IsCompleted)
            {
                return directNext;
            }
        }

        return helixItems.FirstOrDefault(
            helix =>
                helix != null &&
                helix.ID > currentID &&
                !helix.IsCompleted
        );
    }

#if UNITY_EDITOR
    [ContextMenu("Generate GUID")]
    private void GenerateGUID()
    {
        moduleID = System.Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }

    [ContextMenu("Find Helix Items")]
    private void FindHelixItems()
    {
        helixItems = GetComponentsInChildren<HelixItem>(true)
            .OrderBy(helix => helix.ID)
            .ToArray();

        UnityEditor.EditorUtility.SetDirty(this);
    }

    private void OnValidate()
    {
        if (helixItems == null)
            return;

        HelixItem[] validHelixes = helixItems
            .Where(helix => helix != null)
            .ToArray();

        IEnumerable<IGrouping<int, HelixItem>> duplicates =
            validHelixes
                .GroupBy(helix => helix.ID)
                .Where(group => group.Count() > 1);

    }
#endif
}

