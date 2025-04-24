using System;
using UnityEngine;

public class DebugGUI : MonoBehaviour
{
    private SimpleController controller;

    private void Awake()
    {
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<SimpleController>();
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
        GUILayout.Label($"Current Depth : {controller.CurrentDepth}");
        GUILayout.EndArea();
    }
}
