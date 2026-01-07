using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class JsonSplineImporter : EditorWindow
{
    public TextAsset jsonFile;
    public GameObject targetParent; // Optional: put your road mesh here
    public float minPointDistance = 0.05f;

    [MenuItem("Tools/Spline/Import JSON Spline")]
    public static void ShowWindow()
    {
        GetWindow<JsonSplineImporter>("JSON Spline Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("Spline JSON Import", EditorStyles.boldLabel);

        jsonFile = (TextAsset)EditorGUILayout.ObjectField("JSON File", jsonFile, typeof(TextAsset), false);
        targetParent = (GameObject)EditorGUILayout.ObjectField("Parent (optional)", targetParent, typeof(GameObject), true);

        minPointDistance = EditorGUILayout.FloatField(new GUIContent("Min Point Distance", "Minimum distance between points.\n0 disables filtering."), minPointDistance);

        if (minPointDistance < 0f)
            minPointDistance = 0f;

        if (jsonFile != null && GUILayout.Button("Import Spline"))
        {
            ImportSpline(jsonFile, targetParent);
        }
    }

    // ===============================================================
    //  MAIN IMPORT FUNCTION
    // ===============================================================

    void ImportSpline(TextAsset json, GameObject parent)
    {
        if (json == null)
        {
            Debug.LogError("No JSON file assigned.");
            return;
        }

        SplineJson data = JsonUtility.FromJson<SplineJson>(json.text);

        if (data == null || data.points == null || data.points.Count < 2)
        {
            Debug.LogError("Invalid JSON format or not enough points (need at least 2).");
            return;
        }

        // --------------------------
        // CREATE GAMEOBJECT
        // --------------------------
        GameObject go = new GameObject(string.IsNullOrEmpty(data.name) ? "ImportedSpline" : data.name);

        if (parent != null)
        {
            go.transform.SetParent(parent.transform, false); // keep local reset
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(-90, 0, 180); // keep your current setup
            go.transform.localScale = Vector3.one;
        }

        // Layer "Rail"
        int railLayer = LayerMask.NameToLayer("Rail");
        if (railLayer != -1)
            go.layer = railLayer;
        else
            Debug.LogWarning("Layer 'Rail' not found. Create it in Project Settings > Tags and Layers.");

        // --------------------------
        // SPLINE CONTAINER + SPLINE
        // --------------------------
        // --------------------------
        // SPLINE CONTAINER + SPLINE
        // --------------------------
        SplineContainer container = go.AddComponent<SplineContainer>();
        Spline spline = new Spline();

        // Collect positions (Blender x,y,z -> Unity x,z,y)
        List<Vector3> rawPositions = new List<Vector3>(data.points.Count);
        foreach (var p in data.points)
            rawPositions.Add(new Vector3(p.x, p.z, p.y));

        // 🔥 FIX CRITIQUE
        List<Vector3> positions = minPointDistance > 0f ? FilterClosePoints(rawPositions, minPointDistance): rawPositions;


        if (positions.Count < 2)
        {
            Debug.LogError("Need at least 2 points to build a spline.");
            DestroyImmediate(go);
            return;
        }

        // Add knots (no manual tangents)
        for (int i = 0; i < positions.Count; i++)
            spline.Add(new BezierKnot(positions[i]));

        // Force Auto tangents
        for (int i = 0; i < spline.Count; i++)
            spline.SetTangentMode(i, TangentMode.AutoSmooth);

        // 🔥 ANTI-TORSION: force knot rotations with a fixed Up
        Vector3 up = Vector3.up;

        for (int i = 0; i < spline.Count; i++)
        {
            Vector3 t; // tangent direction
            if (i == 0)
                t = (positions[i + 1] - positions[i]);
            else if (i == positions.Count - 1)
                t = (positions[i] - positions[i - 1]);
            else
                t = (positions[i + 1] - positions[i - 1]);

            if (t.sqrMagnitude < 1e-10f)
                t = Vector3.forward;

            t.Normalize();

            // If tangent is almost parallel to up, pick another up to avoid flips
            if (Mathf.Abs(Vector3.Dot(t, up)) > 0.98f)
                up = Vector3.right;

            var k = spline[i];

            // Depending on your Splines version, property is Rotation or rotation.
            // This one matches recent APIs:
            k.Rotation = Quaternion.LookRotation(t, up);

            spline[i] = k;
        }

        spline.Closed = false;
        container.Spline = spline;

        // --------------------------
        // EXTRUDE + RENDER + COLLIDER + RAIL
        // --------------------------
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        SplineExtrude extrude = go.AddComponent<SplineExtrude>();
        extrude.Radius = 0.06f;
        extrude.Sides = 12;
        extrude.Capped = true;


        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.convex = false;

        go.AddComponent<Rail>();

        // --------------------------
        // SELECT IN HIERARCHY
        // --------------------------
        Selection.activeGameObject = go;

        Debug.Log($"Imported spline '{data.name}' with {positions.Count} points (Auto tangents).");
    }

    // ===============================================================
    //  JSON DATA STRUCTURES
    // ===============================================================

    [System.Serializable]
    public class Point
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    public class SplineJson
    {
        public string name;
        public List<Point> points;
    }

    static List<Vector3> FilterClosePoints(List<Vector3> input, float minDist)
    {
        List<Vector3> result = new List<Vector3>();
        if (input.Count == 0) return result;

        result.Add(input[0]);
        Vector3 last = input[0];

        for (int i = 1; i < input.Count; i++)
        {
            if (Vector3.Distance(last, input[i]) >= minDist)
            {
                result.Add(input[i]);
                last = input[i];
            }
        }

        return result;
    }

}
