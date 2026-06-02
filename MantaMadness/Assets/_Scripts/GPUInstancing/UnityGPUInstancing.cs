using UnityEngine;


public class UnityGPUInstancing : MonoBehaviour
{
    public int instanceCount = 10000;
    public Mesh mesh;
    public Material material;
    public Transform center;
    public Vector3 boundSize;

    private UnityEngine.Rendering.ShadowCastingMode castShadows = UnityEngine.Rendering.ShadowCastingMode.Off;
    private bool receiveShadows = false;

    private RenderParams rParams;

    private MaterialPropertyBlock MPB;
    [SerializeField]private Bounds bounds;

    public MeshFilter spawnPlane;

    [Header("Random Scale")]
    [SerializeField] private Vector2 widthRange = new Vector2(0.8f, 1.2f);
    [SerializeField] private Vector2 heightRange = new Vector2(0.5f, 2f);

    /*
    (These properties are used in SetupInstances() function defined later.
    Just allows me to use that snippet in multiple Unity versions...
    You can just reference rParams in the function instead if you prefer)
    */
    #region Instances
    //[LayoutKind.Sequential]
    private struct InstanceData
    {
        public Matrix4x4 matrix;
        //public Color color;

        public static int Size()
        {
            return
                sizeof(float) * 4 * 4   // matrix
                                        //+ sizeof(float) * 4 		// color
            ;
            // Alternatively one of these might work to calculate the size automatically?
            // return System.Runtime.InteropServices.Marshal.SizeOf(typeof(InstanceData));
            // return Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<InstanceData>();
        }
        /*
			Must match the layout/size of the struct in shader
			See https://docs.unity3d.com/ScriptReference/ComputeBufferType.Structured.html
			To avoid issues with how different graphics APIs structure data :
			- Order by largest to smallest 
			- Use Vector4/Color/float4 & Matrix4x4/float4x4 instead of float3 & float3x3
		*/
    }
    private ComputeBuffer instancesBuffer;

    private void SetupInstances()
    {
        if (instanceCount <= 0)
        {
            // Avoid negative or 0 instances, as that will crash Unity
            instanceCount = 1;
        }
        InstanceData[] instances = new InstanceData[instanceCount];
        Vector3 boundsSize = bounds.size;
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 position = GetRandomPointOnPlaneMesh();

            Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            Vector3 scale = new Vector3(
                Random.Range(widthRange.x, widthRange.y),
                Random.Range(heightRange.x, heightRange.y),
                Random.Range(widthRange.x, widthRange.y));
            
            position.y += scale.y * 0.5f;

            instances[i] = new InstanceData
            {
                matrix = Matrix4x4.TRS(position, rotation, scale)
            };
        }
        instancesBuffer = new ComputeBuffer(instanceCount, InstanceData.Size());
        instancesBuffer.SetData(instances);
        MPB.SetBuffer("_PerInstanceData", instancesBuffer);
    }
    #endregion

    void OnEnable()
    {
        Renderer planeRenderer = spawnPlane.GetComponent<Renderer>();

        rParams = new RenderParams(material)
        {

            worldBounds = planeRenderer.bounds,
            shadowCastingMode = castShadows,
            receiveShadows = receiveShadows,
            matProps = new MaterialPropertyBlock()
        };

        bounds = rParams.worldBounds;
        MPB = rParams.matProps;

        SetupInstances();
    }

    void Update()
    {
        if (instanceCount <= 0) return;

        Graphics.RenderMeshPrimitives(rParams, mesh, 0, instanceCount);
    }

    void OnDisable()
    {
        if (instancesBuffer != null)
        {
            instancesBuffer.Release();
            instancesBuffer = null;
        }
    }

    private Vector3 GetRandomPointOnPlaneMesh()
    {
        Mesh planeMesh = spawnPlane.sharedMesh;

        Vector3[] vertices = planeMesh.vertices;
        int[] triangles = planeMesh.triangles;

        int triIndex = Random.Range(0, triangles.Length / 3) * 3;

        Vector3 a = vertices[triangles[triIndex]];
        Vector3 b = vertices[triangles[triIndex + 1]];
        Vector3 c = vertices[triangles[triIndex + 2]];

        // Random barycentric point inside triangle
        float r1 = Random.value;
        float r2 = Random.value;

        if (r1 + r2 > 1f)
        {
            r1 = 1f - r1;
            r2 = 1f - r2;
        }

        Vector3 localPoint = a + r1 * (b - a) + r2 * (c - a);

        // Convertit du local du plane vers world
        Vector3 worldPoint = spawnPlane.transform.TransformPoint(localPoint);

        // Ton shader attend une position relative au centre du bounds
        return worldPoint - bounds.center;
    }
}
