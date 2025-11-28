using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using System.Collections.Generic;

public class MeshBorderToSpline : EditorWindow
{
    GameObject borderMesh;
    int sampleCount = 64;

    [MenuItem("Tools/Spline/Create Spline From Border Mesh")]
    public static void ShowWindow()
    {
        GetWindow<MeshBorderToSpline>("Border → Spline");
    }

    void OnGUI()
    {
        borderMesh = (GameObject)EditorGUILayout.ObjectField("Border Mesh", borderMesh, typeof(GameObject), true);
        sampleCount = EditorGUILayout.IntSlider("Samples", sampleCount, 4, 512);

        if (borderMesh == null) return;

        if (GUILayout.Button("Generate Spline"))
            GenerateSpline(borderMesh, sampleCount);
    }

    static void GenerateSpline(GameObject borderObj, int samples)
    {
        MeshFilter mf = borderObj.GetComponent<MeshFilter>();
        if (!mf)
        {
            Debug.LogError("Selected object has no MeshFilter.");
            return;
        }

        Mesh mesh = mf.sharedMesh;
        if (!mesh || mesh.vertexCount < 3)
        {
            Debug.LogError("Mesh invalid or does not contain enough vertices.");
            return;
        }

        // Collect vertex positions in world-space
        List<Vector3> pts = new List<Vector3>();
        foreach (Vector3 v in mesh.vertices)
            pts.Add(borderObj.transform.TransformPoint(v));

        // Sort points to form a clean loop path
        List<Vector3> sorted = SortLoop(pts);

        // Downsample
        List<Vector3> sampled = Resample(sorted, samples);

        // Create spline object
        GameObject splineObj = new GameObject(borderObj.name + "_Spline");
        var container = splineObj.AddComponent<SplineContainer>();

        Spline spline = new Spline();
        foreach (var p in sampled)
            spline.Add(new BezierKnot(p));

        container.Splines = new List<Spline> { spline };

        Selection.activeObject = splineObj;
    }

    static List<Vector3> SortLoop(List<Vector3> pts)
    {
        // Simple nearest-neighbor loop sort
        List<Vector3> result = new List<Vector3>();
        HashSet<int> used = new HashSet<int>();

        int current = 0;
        result.Add(pts[current]);
        used.Add(current);

        while (used.Count < pts.Count)
        {
            float bestDist = float.MaxValue;
            int bestIdx = -1;

            for (int i = 0; i < pts.Count; i++)
            {
                if (used.Contains(i)) continue;

                float d = Vector3.Distance(pts[current], pts[i]);
                if (d < bestDist)
                {
                    bestDist = d;
                    bestIdx = i;
                }
            }

            current = bestIdx;
            used.Add(current);
            result.Add(pts[current]);
        }

        return result;
    }

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
}