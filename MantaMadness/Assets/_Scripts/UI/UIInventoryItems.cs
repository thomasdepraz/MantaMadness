using System;
using UnityEngine;

public enum UiInventoryType
{
    Ability,
    Key,
}

public class UIInventoryItems : MonoBehaviour
{
    [System.Serializable]
    public struct UIInventoryMeshes
    {
        public GameObject[] activeRenderer;
        public GameObject disabledRenderer;
    }

    public string id;
    public UiInventoryType type;
    public UIInventoryMeshes meshRenderer;

    public void EnableVisual()
    {
        foreach (GameObject renderer in meshRenderer.activeRenderer)
        {
            renderer.SetActive(true);
        }

        meshRenderer.disabledRenderer.SetActive(false);
    }

    public void DisableVisual()
    {
        foreach (GameObject renderer in meshRenderer.activeRenderer)
        {
            renderer.SetActive(false);
        }

        meshRenderer.disabledRenderer.SetActive(true);
    }
}
