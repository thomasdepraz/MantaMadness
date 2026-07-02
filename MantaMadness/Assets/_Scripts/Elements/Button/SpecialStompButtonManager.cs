using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class SpecialStompButtonManager : MonoBehaviour, IDataPersistence
{
    [SerializeField] protected List<SpecialStompButton> buttons = new();

    protected int activatedCount;

    [SerializeField] protected UnityEvent onAllActivated;

    [Header("Parameter")]
    public float spawnTime = 0.05f;

    public CinemachineCamera vcam;
    public CinemachineBlendDefinition blend;

    protected WaitForSeconds wait;
    protected Coroutine routine;

    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;

    protected const float c_lockDuration = 4f;

    private SimpleController controller;

    [SerializeField] private string puzzleID;
    private bool isCompleted = false;

    protected virtual void Start()
    {
        if(controller == null)
        {
            controller = Game.Instance.player;
        }

        foreach (var button in buttons)
        {
            if (button == null)
            {
                Debug.LogWarning("Missing destructible in manager list", this);
                continue;
            }
            button.SetManager(this);
        }

        if (isCompleted)
        {
            ApplyCompletedState();
            return;
        }
        else
        {
            foreach (GameObject objects in objectsToActivate)
            {
                objects.SetActive(false);
            }

            foreach (GameObject objects in objectsToDeactivate)
            {
                objects.SetActive(true);
            }
        }
    }

    public void LoadData(GameData data)
    {
        if (data.puzzleElements.TryGetValue(puzzleID, out bool completed))
        {
            isCompleted = completed;

            if (isCompleted)
            {
                ApplyCompletedState();
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data.puzzleElements.ContainsKey(puzzleID))
        {
            data.puzzleElements[puzzleID] = isCompleted;
        }
        else
        {
            data.puzzleElements.Add(puzzleID, isCompleted);
        }
    }

    public virtual void RegisterDestruction(SpecialStompButton button)
    {
        if (!buttons.Contains(button))
            return;

        activatedCount++;

        Debug.Log($"Destroyed {activatedCount}/{buttons.Count}");

        if (activatedCount >= buttons.Count)
        {
            ActivateEvent();
        }
    }

    protected virtual void ActivateEvent()
    {
        Debug.Log("Module completed!");

        isCompleted = true;

        onAllActivated?.Invoke();
    }

    public void StartEndSequence()
    {
        Debug.Log("Special button manager tu fous quoi ?");
        routine = StartCoroutine(EndSequence());
    }

    private IEnumerator EndSequence()
    {
        yield return new WaitForSeconds(0.75f);

        controller.ForceLock(true);

        //activate camera

        vcam.enabled = true;
        CameraManager.Instance.BlendToCamera(vcam, blend);

        yield return new WaitForSeconds(c_lockDuration / 2);


        foreach (GameObject objects in objectsToDeactivate)
        {
            objects.SetActive(false);
        }

        if (objectsToActivate.Length > 0)
        {
            for (int i = 0; i < objectsToActivate.Length; i++)
            {
                //ACTIVATE OBJECT
                objectsToActivate[i].SetActive(true);
                objectsToActivate[i].transform.DOMoveY(objectsToActivate[i].transform.position.y + 5f, 0.2f).SetEase(Ease.OutQuad).SetLoops(2, LoopType.Yoyo);
                yield return new WaitForSeconds(spawnTime / objectsToActivate.Length);
            }
        }

        yield return new WaitForSeconds(c_lockDuration / 2);

        // PART 3 RESET BACK TO NORMAL

        //unlock player
        controller.ForceLock(false);

        //reset camera
        CameraManager.Instance.ResetCamera(vcam);

        routine = null;
        yield return null;
    }

    private void ApplyCompletedState()
    {
        // désactiver anciens objets
        foreach (GameObject obj in objectsToDeactivate)
        {
            obj.SetActive(false);
        }

        // activer nouveaux
        foreach (GameObject obj in objectsToActivate)
        {
            obj.SetActive(true);
        }
    }

}
