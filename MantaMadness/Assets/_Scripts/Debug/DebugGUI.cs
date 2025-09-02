using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class DebugGUI : MonoBehaviour
{
    private SimpleController controller;
    List<ScriptableRendererFeature> features = new List<ScriptableRendererFeature>();

    private void Awake()
    {
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<SimpleController>();
        features = RenderFeatureUtility.GetRenderFeatures();
    }

#if UNITY_EDITOR
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
            var feature = RenderFeatureUtility.GetFeature(features, "CRT");
            if (feature != null)
                feature.SetActive(!feature.isActive);
        }
        if (GUILayout.Button($"Set Pixelize"))
        {
            var feature = RenderFeatureUtility.GetFeature(features, "Pixelize");
            if (feature != null)
                feature.SetActive(!feature.isActive);
        }
        if (GUILayout.Button($"Set UnderwaterMusic"))
        {
            MusicManager.Instance.ToggleUnderwater();
        }

        if (GUILayout.Button($"Toggle Music"))
        {
            MusicManager.Instance.ToggleMusic();
        }
        GUILayout.EndArea();
    }
#endif
}
