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
        public MeshRenderer renderer;
        public Material defaultMaterial;
        public Material disableMaterial;
    }

    public string id;
    public UiInventoryType type;
    public UIInventoryMeshes[] meshRenderers;
}
