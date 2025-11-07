using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager instance;

    [SerializeField] private List<DialogSequence> dialogSequences;

    [SerializeField] private DialogSequence currentSequence;

    [SerializeField] private GameObject[] dialogUIVisuals;

    [SerializeField] private TextMeshProUGUI speakerTextBox;
    [SerializeField] private TextMeshProUGUI dialogTextBox;

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
        speakerTextBox = GameObject.Find("DialogSpeaker").GetComponent<TextMeshProUGUI>();
        dialogTextBox = GameObject.Find("DialogContent").GetComponent<TextMeshProUGUI>();
        foreach (GameObject visual in dialogUIVisuals)
        {
            visual.SetActive(false);
        }
    }

    public void StartSequence(string sequenceKey)
    {
        foreach (DialogSequence dialogSequence in dialogSequences)
        {
            if(dialogSequence.sequenceKey == sequenceKey)
            {
                Debug.Log("ENTERED");
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
    }

    public IEnumerator Dialog(DialogAsset dialog)
    {
        //TODO Switch Cam
        //TODO LOCK PLAYER / player inputs (pause et autres)
        //TODO Disable regular UI

        yield return new WaitForSeconds(dialog.delayBeforeTextBox);

        foreach(GameObject visual in dialogUIVisuals)
        {
            visual.SetActive(true);
        }

        //If text box is not show > Tween in textbox
        speakerTextBox.text = dialog.speakerName;

        //TODO text defilement script / text = dialog.text
        dialogTextBox.text = dialog.dialogText;
        //TODO Wait until player input / yield return new WaitUntil
        //TODO if dialogwrite not ended = show full dialogs then WaitUntil again
        yield return new WaitForSeconds(dialog.delayBeforeTextBox);
        currentSequenceCount++;
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
    }
}
