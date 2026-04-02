using UnityEngine;

public class FogTypeClass : MonoBehaviour
{
    public enum FogType
    {
        Close,
        Far,
        Special
    }

    [SerializeField] public FogType type;
    [SerializeField] public Material fogMat;

    private void Awake()
    {
        fogMat = GetComponent<MeshRenderer>().material;
    }
}
