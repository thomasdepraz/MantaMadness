using TMPro;
using UnityEngine;

public class UiTutorialManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField] private TutorialAsset[] tutorialAssets;

    private void Start()
    {
        UIManager.Instance.uiTutorialManager= this;
        ResetTutorialText();

        foreach (TutorialAsset asset in tutorialAssets)
        {
            var entry = DialogLoader.GetText(asset.key);
            asset.dialogText = entry.dialog;
        }
    }


    public void ResetTutorialText()
    {
        tutorialText.gameObject.SetActive(false);
    }

    public void SetText(TutorialType tutoType)
    {

        foreach(TutorialAsset asset in tutorialAssets)
        {
            if(asset.type == tutoType)
            {
                string parsedText = DialogLoader.ParseInputs(asset.dialogText);
                tutorialText.text = parsedText;
                tutorialText.font = asset.dialogMat;
            }
        }
        tutorialText.gameObject.SetActive(true);
    }

}
