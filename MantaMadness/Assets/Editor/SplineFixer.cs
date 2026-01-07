using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public static class SplineFixer
{
    [MenuItem("Tools/Splines/Fix Broken Tangents (Auto)")]
    static void FixBrokenTangents()
    {
        var containers = Object.FindObjectsByType<SplineContainer>(
            FindObjectsSortMode.None
        );

        int fixedCount = 0;

        foreach (var container in containers)
        {
            foreach (var spline in container.Splines)
            {
                for (int i = 0; i < spline.Count; i++)
                {
                    var mode = spline.GetTangentMode(i);

                    if (mode == TangentMode.Broken)
                    {
                        spline.SetTangentMode(i, TangentMode.AutoSmooth);
                        fixedCount++;
                    }
                }
            }

            EditorUtility.SetDirty(container);
        }

        Debug.Log($"Splines corrigées : {fixedCount} point(s) Broken → Auto");
    }
}
