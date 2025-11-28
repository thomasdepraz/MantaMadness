using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using System.Collections.Generic;

public class RoadBorderSplineGenerator : EditorWindow
{
    GameObject roadMesh;
    int sampleCount = 64;

    [MenuItem("Tools/Spline/Generate Road Border Splines")]
    public static void ShowWindow()
    {
        GetWindow<RoadBorderSplineGenerator>("Road → Border Splines");
    }

    void OnGUI()
    {
        roadMesh = (GameObject)EditorGUILayout.ObjectField("Road Mesh", roadMesh, typeof(GameObject), true);
        sampleCount = EditorGUILayout.IntSlider("Samples per Side", sampleCount, 8, 512);

        if (roadMesh == null) return;

        if (GUILayout.Button("Generate Left & Right Splines"))
            Generate(roadMesh, sampleCount);
    }

    // -----------------------------------------------------
    // MAIN GENERATION
    // -----------------------------------------------------
    static void Generate(GameObject meshObj, int samples)
    {
        MeshFilter mf = meshObj.GetComponent<MeshFilter>();
        if (!mf)
        {
            Debug.LogError("Selected object has no MeshFilter.");
            return;
        }

        Mesh mesh = mf.sharedMesh;
        if (!mesh)
        {
            Debug.LogError("MeshFilter has no mesh.");
            return;
        }

        // 1. Extract boundary loops
        List<List<int>> borderLoops = ExtractBorderLoops(mesh);

        if (borderLoops.Count != 2)
        {
            Debug.LogError("Expected 2 border edges, but found: " + borderLoops.Count);
            return;
        }

        // 2. Convert vertex indices to world positions
        List<Vector3> sideA = IndicesToWorld(meshObj, mesh, borderLoops[0]);
        List<Vector3> sideB = IndicesToWorld(meshObj, mesh, borderLoops[1]);

        // 3. Clean loops (sort points)
        sideA = SortLoop(sideA);
        sideB = SortLoop(sideB);

        // 4. Downsample
        sideA = Resample(sideA, samples);
        sideB = Resample(sideB, samples);

        // 5. Create splines
        CreateSpline(meshObj, sideA, meshObj.name + "_BorderLeft_Spline");
        CreateSpline(meshObj, sideB, meshObj.name + "_BorderRight_Spline");

        Debug.Log("Generated Left & Right splines for road: " + meshObj.name);
    }

    // -----------------------------------------------------
    // Extract boundary edge loops
    // -----------------------------------------------------
    static List<List<int>> ExtractBorderLoops(Mesh mesh)
    {
        Dictionary<(int, int), int> edgeCount = new Dictionary<(int, int), int>();

        int[] tris = mesh.triangles;
        Vector3[] verts = mesh.vertices;

        // Count edges inside triangles
        for (int i = 0; i < tris.Length; i += 3)
        {
            int a = tris[i];
            int b = tris[i + 1];
            int c = tris[i + 2];

            AddEdge(a, b, edgeCount);
            AddEdge(b, c, edgeCount);
            AddEdge(c, a, edgeCount);
        }

        // Boundary edges = edges referenced once
        HashSet<int> borderVerts = new HashSet<int>();

        foreach (var kv in edgeCount)
        {
            if (kv.Value == 1)
            {
                borderVerts.Add(kv.Key.Item1);
                borderVerts.Add(kv.Key.Item2);
            }
        }

        // Group into loops
        return GroupIntoLoops(mesh, borderVerts);
    }

    static void AddEdge(int a, int b, Dictionary<(int, int), int> dict)
    {
        var key = (Mathf.Min(a, b), Mathf.Max(a, b));
        if (!dict.ContainsKey(key)) dict[key] = 0;
        dict[key]++;
    }

    static List<List<int>> GroupIntoLoops(Mesh mesh, HashSet<int> borderVerts)
    {
        Dictionary<int, List<int>> adjacency = new Dictionary<int, List<int>>();

        int[] tris = mesh.triangles;
        for (int i = 0; i < tris.Length; i += 3)
        {
            int[] t = { tris[i], tris[i + 1], tris[i + 2] };

            for (int a = 0; a < 3; a++)
            {
                int v1 = t[a];
                int v2 = t[(a + 1) % 3];

                if (!borderVerts.Contains(v1) || !borderVerts.Contains(v2)) continue;

                if (!adjacency.ContainsKey(v1)) adjacency[v1] = new List<int>();
                if (!adjacency.ContainsKey(v2)) adjacency[v2] = new List<int>();

                if (!adjacency[v1].Contains(v2)) adjacency[v1].Add(v2);
                if (!adjacency[v2].Contains(v1)) adjacency[v2].Add(v1);
            }
        }

        // Extract loops
        List<List<int>> loops = new List<List<int>>();
        HashSet<int> visited = new HashSet<int>();

        foreach (int start in borderVerts)
        {
            if (visited.Contains(start)) continue;

            List<int> loop = new List<int>();
            int current = start;
            int prev = -1;

            while (!visited.Contains(current))
            {
                visited.Add(current);
                loop.Add(current);

                // pick next
                List<int> neighbors = adjacency[current];
                int next = neighbors.Find(n => n != prev);

                prev = current;
                current = next;
            }

            loops.Add(loop);
        }

        return loops;
    }

    // -----------------------------------------------------
    // Convert index loop → world positions
    // -----------------------------------------------------
    static List<Vector3> IndicesToWorld(GameObject meshObj, Mesh mesh, List<int> indices)
    {
        List<Vector3> pts = new List<Vector3>();
        foreach (int i in indices)
        {
            pts.Add(meshObj.transform.TransformPoint(mesh.vertices[i]));
        }
        return pts;
    }

    // -----------------------------------------------------
    // Sort a border loop sequentially
    // -----------------------------------------------------
    static List<Vector3> SortLoop(List<Vector3> pts)
    {
        List<Vector3> sorted = new List<Vector3>();
        HashSet<int> used = new HashSet<int>();

        int current = 0;
        sorted.Add(pts[current]);
        used.Add(current);

        while (used.Count < pts.Count)
        {
            float best = float.MaxValue;
            int bestIdx = -1;

            for (int i = 0; i < pts.Count; i++)
            {
                if (used.Contains(i)) continue;

                float d = Vector3.Distance(pts[current], pts[i]);
                if (d < best)
                {
                    best = d;
                    bestIdx = i;
                }
            }

            current = bestIdx;
            used.Add(current);
            sorted.Add(pts[current]);
        }

        return sorted;
    }

    // -----------------------------------------------------
    // Downsample a loop
    // -----------------------------------------------------
    static List<Vector3> Resample(List<Vector3> pts, int target)
    {
        List<Vector3> r = new List<Vector3>();
        for (int i = 0; i < target; i++)
        {
            float t = (float)i / (target - 1);
            float f = t * (pts.Count - 1);
            int i0 = Mathf.FloorToInt(f);
            int i1 = Mathf.Min(i0 + 1, pts.Count - 1);

            r.Add(Vector3.Lerp(pts[i0], pts[i1], f - i0));
        }
        return r;
    }

    // -----------------------------------------------------
    // Create a new spline object
    // -----------------------------------------------------
    static void CreateSpline(GameObject road, List<Vector3> pts, string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetPositionAndRotation(road.transform.position, road.transform.rotation);
        go.transform.localScale = road.transform.localScale;

        var container = go.AddComponent<SplineContainer>();
        Spline spline = new Spline();

        foreach (var p in pts)
            spline.Add(new BezierKnot(p));

        container.Splines = new List<Spline>() { spline };
    }
}

