using DG.Tweening;
using DG.Tweening.Core;
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

    [SerializeField] private DialogSequence currentSequence;

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
        inputs.jump.action.performed += Interacts;

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
        inputs.jump.action.performed -= Interacts;
    }

    private void Interacts(InputAction.CallbackContext context)
    {
        interacted = true;
        print("Has interacted");
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

        PlayDialog();
    }
    public void PlayDialog()
    {
        StartCoroutine(Dialog(currentSequence.sequence[currentSequenceCount]));
        Game.Instance.player.ToggleDialogState(true);
    }

    public IEnumerator Dialog(DialogAsset dialog)
    {
        //TODO Switch Cam
        //TODO LOCK PLAYER / player inputs (pause et autres)
        //TODO Disable regular UI
        UIManager.Instance.ToggleBaseInterface(false);
        yield return new WaitForSeconds(dialog.delayBeforeTextBox);

        foreach(GameObject visual in dialogUIVisuals)
        {
            visual.SetActive(true);
        }

        //If text box is not show > Tween in textbox
        speakerTextBox.text = dialog.speakerName;

        //TODO text defilement script / text = dialog.text
        yield return StartCoroutine(TypeText(dialog));
        //TODO Wait until player input / yield return new WaitUntil
    }

    private IEnumerator TypeText(DialogAsset dialog)
    {
        isTyping = true;
        dialogTextBox.text = dialog.dialogText;

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
        yield return new WaitUntil(() => dialogTextBox.text == dialog.dialogText && dialogWriter.IsWriting == false);

        //Enable indicator visual
        dialogIndicator.DOFade(1, 0.5f).SetLoops(-1,LoopType.Yoyo);

        isTyping = false;

        yield return StartCoroutine(EndDialog());
    }

    private IEnumerator EndDialog()
    {
        yield return new WaitUntil(() => interacted == true);
        interacted = false;
        currentSequenceCount++;

        //Disable indicator visual
        dialogIndicator.DOKill();
        dialogIndicator.color = new Color(255, 255, 255, 0);
        if (currentSequenceCount >= currentSequence.sequence.Length)
        {
            print("Current count" + currentSequenceCount);
            print("Sequence length" + currentSequence.sequence.Length);
            ResetSequence();
            yield return null;
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
