using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    private const string MAIN_SCENE_NAME = "Main";
    private const float READY_PROGRESS = 0.9f;

    private void Start()
    {
        StartCoroutine(LoadMainScene());
    }

    private IEnumerator LoadMainScene()
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(MAIN_SCENE_NAME);
        if (loadOperation == null)
        {
            yield break;
        }

        loadOperation.allowSceneActivation = false;

        while (loadOperation.progress < READY_PROGRESS)
        {
            yield return null;
        }

        loadOperation.allowSceneActivation = true;
    }
}
