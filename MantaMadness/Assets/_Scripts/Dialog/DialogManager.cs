using DG.Tweening;
using DG.Tweening.Core;
using FMOD.Studio;
using FMODUnity;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPEffects.Components;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Windows;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance;

    [SerializeField] private List<DialogSequence> dialogSequences;

    [SerializeField] public DialogSequence currentSequence { get; private set; }

    [SerializeField] private GameObject[] dialogUIVisuals;

    [SerializeField] private TextMeshProUGUI speakerTextBox;
    [SerializeField] private TextMeshProUGUI dialogTextBox;
    [SerializeField] private TMPWriter dialogWriter;
    [SerializeField] private Image dialogIndicator;

    [Header("Parameters")]
    public float typingSpeed = 0.25f;

    private bool isTyping = false;
    private bool skipTyping = false;

    private InputManager inputs;
    private bool interacted = false;

    public EventReference dialogActiveReference;
    public FMOD.Studio.EventInstance dialogActiveEvent;

    private InteractableNPC currentNpc;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        inputs = InputManager.Instance;
        inputs.interact.action.performed += Interacts;
        inputs.interact.action.performed += StartNPCInteraction;

        speakerTextBox = GameObject.Find("DialogSpeaker").GetComponent<TextMeshProUGUI>();
        dialogTextBox = GameObject.Find("DialogContent").GetComponent<TextMeshProUGUI>();
        dialogIndicator = GameObject.Find("DialogIndicator").GetComponent<Image>();
        dialogIndicator.color = new Color(255, 255, 255, 0);
        foreach (GameObject visual in dialogUIVisuals)
        {
            visual.SetActive(false);
        }

        foreach(DialogSequence asset in dialogSequences)
        {
            for (int i = 0; i < asset.sequence.Length; i++)
            {
                var entry = DialogLoader.GetText(asset.sequence[i].key);

                asset.sequence[i].dialogText = entry.dialog;
                asset.sequence[i].speakerName = entry.speaker;
            }
        }
    }

    private void OnDisable()
    {
        inputs.interact.action.performed -= Interacts;
        inputs.interact.action.performed -= StartNPCInteraction;
    }

    private void Interacts(InputAction.CallbackContext context)
    {
        interacted = true;
        print("Has interacted");
    }

    private void StartNPCInteraction(InputAction.CallbackContext context)
    {
        if(currentSequence == null)
        {
            List<Collider> npc = CameraTargetDetection.Instance.validNPCTargets;

            float closestNpcDistance = 0f;

            InteractableNPC selectedNpc = null;

            if (npc.Count > 1)
            {
                for (int i = 0; i < npc.Count; i++)
                {
                    if (i == 0)
                    {
                        closestNpcDistance = Vector3.Distance(CameraTargetController.instance.transform.position, npc[i].transform.position);
                        selectedNpc = npc[i].GetComponent<InteractableNPC>();
                    }
                    else
                    {
                        if (closestNpcDistance > Vector3.Distance(CameraTargetController.instance.transform.position, npc[i].transform.position))
                        {
                            closestNpcDistance = Vector3.Distance(CameraTargetController.instance.transform.position, npc[i].transform.position);
                            selectedNpc = npc[i].GetComponent<InteractableNPC>();
                        }
                    }
                }
            }
            else if (npc.Count == 1)
            {
                selectedNpc = npc[0].GetComponent<InteractableNPC>();
            }
            else
            {
                Debug.Log("NPC in range list is empty");
            }

            if (selectedNpc != null)
            {
                string dialogKey = selectedNpc.GetCurrentDialogKey();

                if (!string.IsNullOrEmpty(dialogKey))
                {
                    currentNpc = selectedNpc;
                    StartSequence(dialogKey);
                }
            }
        }
    }

    public void StartSequence(string sequenceKey)
    {
        foreach (DialogSequence dialogSequence in dialogSequences)
        {
            if(dialogSequence.sequenceKey == sequenceKey)
            {
                if(currentSequence != null)
                {
                    currentSequence = null;
                    currentSequence = dialogSequence;
                    StartDialog();
                }
                else
                {
                    currentSequence = dialogSequence;
                    StartDialog();
                }
            }
        }
    }

    int currentSequenceCount = 0;
    public void StartDialog()
    {
        if (currentSequenceCount != 0)
        {
            currentSequenceCount = 0;
        }
        //START DIALOG FMOD EVENT
        dialogActiveEvent = RuntimeManager.CreateInstance(dialogActiveReference);
        dialogActiveEvent.start();
        PlayDialog();
    }
    public void PlayDialog()
    {
        StartCoroutine(Dialog(currentSequence.sequence[currentSequenceCount]));
        //LOCK PLAYER / player inputs (pause et autres)
        Game.Instance.player.ToggleDialogState(true);
    }

    public IEnumerator Dialog(DialogAsset dialog)
    {
        //Play Cinematic
        if(dialog.cinematic != null)
        CinematicManager.instance.PlayCinematic(dialog.cinematic);

        //Disable regular UI
        UIManager.Instance.ToggleBaseInterface(false);
        yield return new WaitForSeconds(dialog.delayBeforeTextBox);

        foreach(GameObject visual in dialogUIVisuals)
        {
            visual.SetActive(true);
        }

        //If text box is not show > Tween in textbox

        //Set name and name material
        speakerTextBox.text = dialog.speakerName;
        speakerTextBox.font = dialog.speakerMat;

        //Set Dialog material
        dialogTextBox.font = dialog.dialogMat;

        //text defilement script / text = dialog.text
        yield return StartCoroutine(TypeText(dialog));
    }

    private IEnumerator TypeText(DialogAsset dialog)
    {
        isTyping = true;
        string parsedText = DialogLoader.ParseInputs(dialog.dialogText);
        dialogTextBox.text = parsedText;
        RuntimeManager.PlayOneShot(dialog.dialogSound);
        dialogWriter.OnCharacterShown.AddListener(PlaySoundOnCharWritten);
        dialogWriter.StartWriter();

        while(dialogWriter.IsWriting == true)
        {
            if (interacted == true)
            {
                dialogWriter.SkipWriter(true);
                interacted = false;
                break;
            }
            yield return null;
        }
        yield return new WaitUntil(() => dialogTextBox.text == parsedText && dialogWriter.IsWriting == false);

        //Enable indicator visual
        dialogIndicator.DOFade(1, 0.5f).SetLoops(-1,LoopType.Yoyo);

        isTyping = false;

        yield return StartCoroutine(EndDialog());
    }

    private void PlaySoundOnCharWritten(TMPEffects.Components.TMPWriter writer, TMPEffects.CharacterData.CharData c)
    {
        if (c == null) return;

        //if(currentSequence.sequence[currentSequenceCount].dialogSound != null)
        RuntimeManager.PlayOneShot(currentSequence.sequence[currentSequenceCount].dialogSound);
    }

    private IEnumerator EndDialog()
    {
        yield return new WaitUntil(() => interacted == true);
        interacted = false;
        currentSequenceCount++;

        //Disable indicator visual
        dialogIndicator.DOKill();
        dialogIndicator.color = new Color(255, 255, 255, 0);
        CinematicManager.instance.EndCinematic();
        if (currentSequenceCount >= currentSequence.sequence.Length)
        {
            print("Current count" + currentSequenceCount);
            print("Sequence length" + currentSequence.sequence.Length);
            ResetSequence();
        }
        else
        {
            PlayDialog();
            print("Sequence continues");
            yield return null;
        }


    }

    private void ResetSequence()
    {
        Debug.Log("Stop sequence");

        //STOP DIALOG FMOD EVENT
        dialogActiveEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        dialogActiveEvent.release();

        if (currentNpc != null)
        {
            currentNpc.IncrementIndex();
            currentNpc = null;
        }

        currentSequence = null;
        currentSequenceCount = 0;
        foreach (GameObject visual in dialogUIVisuals)
        {
            visual.SetActive(false);
        }

        UIManager.Instance.ToggleBaseInterface(true);
        Game.Instance.player.ToggleDialogState(false);
    }
}
