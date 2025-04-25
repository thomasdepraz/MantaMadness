using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class DebugGUI : MonoBehaviour
{
    private SimpleController controller;
    List<ScriptableRendererFeature> features = new List<ScriptableRendererFeature>();

    private void Awake()
    {
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<SimpleController>();

        ScriptableRenderer renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(0);
        var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
        features = property.GetValue(renderer) as List<ScriptableRendererFeature>;
    }

    private void OnGUI()
    {
        if (controller is null)
            return;

        GUILayout.BeginArea(new Rect(10,10, 200, 500), "Debug", GUI.skin.window);
        GUILayout.Label($"Velocity : {Math.Round(controller.Velocity.magnitude, 2)}");
        GUILayout.Label($"H Velocity : {Math.Round(controller.HorizontalVelocity.magnitude,2)}");
        GUILayout.Label($"y velocity : {Math.Round(controller.Velocity.y,2)}");
        GUILayout.Label($"Angular : {Math.Round(controller.AngularVelocity.magnitude,2)}");
        GUILayout.Label($"CurrentState : {controller.State}");
        GUILayout.Label($"Current Depth : {Math.Round(controller.CurrentDepth,2)}");
        GUILayout.Label($"Max Depth : {Math.Round(controller.MaxDepth,2)}");
        if(GUILayout.Button($"Set CRT"))
        {
            var feature = GetFeature("CRT");
            if (feature != null)
                feature.SetActive(!feature.isActive);
        }
        if (GUILayout.Button($"Set Pixelize"))
        {
            var feature = GetFeature("Pixelize");
            if (feature != null)
                feature.SetActive(!feature.isActive);
        }
        GUILayout.EndArea();
    }

    private ScriptableRendererFeature GetFeature(string name)
    {
        for (int i = 0; i < features.Count; i++)
        {
            if (features[i].name == name)
                return features[i];
        }

        return null;
    }
}
