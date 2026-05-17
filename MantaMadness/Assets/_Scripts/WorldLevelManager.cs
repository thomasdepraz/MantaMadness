using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Splines;

public class WorldLevelManager : MonoBehaviour
{
    public static WorldLevelManager Instance;

    [SerializeField]
    private List<LevelData> allLevels;

    private string currentLevel;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator LoadLevel(string levelID)
    {

        if (currentLevel == levelID)
        {
            Debug.Log($"Level {levelID} already loaded.");
            yield break;
        }

        currentLevel = levelID;

        yield return null;
        
        foreach (LevelData level in allLevels)
        {
            if (level.levelRoots == null)
                continue;

            bool shouldBeActive = level.levelID == levelID;

            foreach(GameObject load in level.levelRoots)
            {
                if (load.activeSelf != shouldBeActive)
                {
                    load.SetActive(shouldBeActive);
                }

            }


            if (level.splinesRoots == null)
                continue;

            foreach (GameObject load in level.splinesRoots)
            {
                foreach (SplineInstantiate spline in load.GetComponentsInChildren<SplineInstantiate>())
                {
                    spline.enabled = shouldBeActive;
                }
            }
        }

        Debug.Log($"Loaded Level : {levelID}");
    }

    public string GetCurrentLevel()
    {
        return currentLevel;
    }
}
