using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

public class GrassPainterTool : EditorWindow
{
    GameObject prefab;
    Transform parent;

    LayerMask paintLayer;

    float brushSize = 5f;
    int density = 10;

    float minScale = 0.8f;
    float maxScale = 1.2f;

    float minDistanceBetween = 0.5f;
    int eraseDensity = 10;

    bool painting = false;
    bool eraser = false;

    [MenuItem("Tools/Grass Painter")]
    public static void OpenWindow()
    {
        GetWindow<GrassPainterTool>("Grass Painter");
    }

    void OnGUI()
    {
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
        parent = (Transform)EditorGUILayout.ObjectField("Parent", parent, typeof(Transform), true);

        paintLayer = LayerMaskField("Paint Layer", paintLayer);

        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 20f);
        density = EditorGUILayout.IntSlider("Density", density, 1, 50);

        minScale = EditorGUILayout.FloatField("Min Scale", minScale);
        maxScale = EditorGUILayout.FloatField("Max Scale", maxScale);

        GUILayout.Space(10);

        minDistanceBetween = EditorGUILayout.FloatField("Min Distance Between", minDistanceBetween);
        eraseDensity = EditorGUILayout.IntSlider("Erase Density", eraseDensity, 1, 50);

        GUILayout.Space(10);

        painting = GUILayout.Toggle(painting, "Enable Painting", "Button");
        eraser = GUILayout.Toggle(eraser, "Eraser Mode", "Button");
    }

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        if (!painting || parent == null)
            return;

        Event e = Event.current;

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1000f, paintLayer))
        {

            Handles.color = eraser
                ? new Color(1, 0, 0, 0.3f)
                : new Color(0, 1, 0, 0.3f);

            Handles.DrawSolidDisc(hit.point, hit.normal, brushSize);

            if (e.type == EventType.MouseDrag && e.button == 0 && !e.alt)
            {
                if (eraser)
                    Erase(hit.point);
                else
                    Paint(hit.point);

                e.Use();
            }
        }
    }


    void Paint(Vector3 center)
    {
        if (prefab == null) return;

        for (int i = 0; i < density; i++)
        {
            Vector2 circle = Random.insideUnitCircle * brushSize;
            Vector3 offset = new Vector3(circle.x, 0, circle.y);

            Vector3 spawnPos = center + offset;

            Ray downRay = new Ray(spawnPos + Vector3.up * 5f, Vector3.down);
            RaycastHit hit;

            if (Physics.Raycast(downRay, out hit, 10f, paintLayer))
            {
                if (!IsFarEnough(hit.point))
                    continue;

                GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

                Undo.RegisterCreatedObjectUndo(obj, "Paint Grass");

                obj.transform.position = hit.point;

                obj.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                float scale = Random.Range(minScale, maxScale);
                obj.transform.localScale = Vector3.one * scale;

                obj.transform.SetParent(parent);
            }
        }
    }

    bool IsFarEnough(Vector3 position)
    {
        foreach (Transform child in parent)
        {
            if (Vector3.Distance(child.position, position) < minDistanceBetween)
                return false;
        }

        return true;
    }

    void Erase(Vector3 center)
    {
        List<Transform> candidates = new List<Transform>();

        foreach (Transform child in parent)
        {
            float dist = Vector3.Distance(child.position, center);

            if (dist <= brushSize)
            {
                candidates.Add(child);
            }
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            int rand = Random.Range(i, candidates.Count);
            var temp = candidates[i];
            candidates[i] = candidates[rand];
            candidates[rand] = temp;
        }

        int count = Mathf.Min(eraseDensity, candidates.Count);

        for (int i = 0; i < count; i++)
        {
            Undo.DestroyObjectImmediate(candidates[i].gameObject);
        }
    }

    LayerMask LayerMaskField(string label, LayerMask mask)
    {
        var layers = InternalEditorUtility.layers;
        int maskWithoutEmpty = 0;

        for (int i = 0; i < layers.Length; i++)
        {
            int layer = LayerMask.NameToLayer(layers[i]);
            if (((1 << layer) & mask.value) > 0)
                maskWithoutEmpty |= 1 << i;
        }

        maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers);

        int newMask = 0;
        for (int i = 0; i < layers.Length; i++)
        {
            if ((maskWithoutEmpty & (1 << i)) > 0)
                newMask |= 1 << LayerMask.NameToLayer(layers[i]);
        }

        mask.value = newMask;
        return mask;
    }
}