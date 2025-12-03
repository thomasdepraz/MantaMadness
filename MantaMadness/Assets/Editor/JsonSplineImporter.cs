using UnityEngine;
using UnityEditor;
using UnityEngine.Splines;
using System.Collections.Generic;

public class JsonSplineImporter : EditorWindow
{
    public TextAsset jsonFile;
    public GameObject targetParent; // Optional: put your road mesh here

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

        if (data == null)
        {
            Debug.LogError("Invalid JSON format.");
            return;
        }

        if (data.points == null || data.points.Count == 0)
        {
            Debug.LogError("JSON contains no points.");
            return;
        }

        // --------------------------
        // CREATE GAMEOBJECT
        // --------------------------
        GameObject go = new GameObject(string.IsNullOrEmpty(data.name) ? "ImportedSpline" : data.name);

        // Parent (sans rotation chelou)
        if (parent != null)
        {
            go.transform.SetParent(parent.transform, false);   // false = localPosition/Rotation/Scale reset
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.Euler(-90, 0, 180);
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
        SplineContainer container = go.AddComponent<SplineContainer>();
        Spline spline = new Spline();

        foreach (var p in data.points)
        {
            // Ton mapping qui marche : Blender (x,y,z) -> Unity (x,z,y)
            Vector3 pos = new Vector3(p.x, p.z, p.y);
            spline.Add(new BezierKnot(pos));
        }

        spline.Closed = false;
        container.Spline = spline;

        // --------------------------
        // EXTRUDE + RENDER + COLLIDER + RAIL
        // --------------------------

        // MeshFilter + Renderer (obligatoires pour l'extrude)
        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        // tu pourras assigner un material dans l’inspecteur
        // mr.sharedMaterial = ...

        // SplineExtrude
        SplineExtrude extrude = go.AddComponent<SplineExtrude>();
        extrude.Radius = 0.06f;
        extrude.Sides = 12;
        extrude.Capped = true;

        // MeshCollider
        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.convex = false;
        // le mesh sera rempli par le SplineExtrude (via MeshFilter); le collider suivra

        // Script Rail
        go.AddComponent<Rail>();

        // --------------------------
        // SELECT IN HIERARCHY
        // --------------------------
        Selection.activeGameObject = go;

        Debug.Log($"Imported spline '{data.name}' with {data.points.Count} points, rail components added.");
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
}
