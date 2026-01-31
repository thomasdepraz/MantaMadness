using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartScreenBehavior : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadingScreen());
    }

    private IEnumerator LoadingScreen()
    {
        yield return new WaitForSeconds(6f);
        SceneManager.LoadScene("MainMenu");
    }
}
