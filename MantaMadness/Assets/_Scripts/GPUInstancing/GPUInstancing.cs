using UnityEngine;

public class GPUInstancing : MonoBehaviour
{
    public Mesh mesh;
    public Material material;


    ComputeBuffer argsBuffer;


    void Start()
    {
        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        uint[] args = new uint[5];
        args[0] = mesh.GetIndexCount(0);
        args[1] = 10000;
        args[2] = mesh.GetIndexStart(0);
        args[3] = mesh.GetBaseVertex(0);
        args[4] = 0;

        argsBuffer.SetData(args);
    }

    void Update()
    {
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(100, 100, 1));
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }
}
