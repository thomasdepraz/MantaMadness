using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class DebugGUI : MonoBehaviour
{
    private SimpleController controller;
    List<ScriptableRendererFeature> features = new List<ScriptableRendererFeature>();

    private bool showDebug = true; // état d'affichage
    public KeyCode toggleKey = KeyCode.F3; // touche pour toggle
    public KeyCode skyKey = KeyCode.K;

    private bool toggleAbility = true;
    private bool visualCountToggle = true;
    private bool trailerVisualToggle = true;
    private bool trailerCamToggle = false;

    private void Awake()
    {
        controller = GameObject.FindGameObjectWithTag("Player").GetComponent<SimpleController>();
        features = RenderFeatureUtility.GetRenderFeatures();
        showDebug = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showDebug = !showDebug;
        }

        if (showDebug && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (!showDebug && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (Input.GetKeyDown(skyKey))
        {
            WeatherManager.instance.DebugSwitchCondition();
        }

    }

    private void OnGUI()
    {
        if (!showDebug || controller == null)
            return;


        GUILayout.BeginArea(new Rect(10, 10, 200, 700), "Debug", GUI.skin.window);
        GUILayout.Label($"Velocity : {Math.Round(controller.Velocity.magnitude, 2)}");
        GUILayout.Label($"H Velocity : {Math.Round(controller.HorizontalVelocity.magnitude, 2)}");
        GUILayout.Label($"y velocity : {Math.Round(controller.Velocity.y, 2)}");
        GUILayout.Label($"Angular : {Math.Round(controller.AngularVelocity.magnitude, 2)}");
        GUILayout.Label($"CurrentState : {controller.State}");
        GUILayout.Label($"Current Depth : {Math.Round(controller.CurrentDepth, 2)}");
        GUILayout.Label($"Max Depth : {Math.Round(controller.MaxDepth, 2)}");
        GUILayout.Label($"Current Collectible Area : {CollectibleAreaManager.CurrentArea}");

        if (GUILayout.Button($"Set CRT"))
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

        if (GUILayout.Button($"Change Music"))
        {
            FmodGlobalParameters.instance.ToggleGlobalParameter(FmodGlobalParamName.G_SecretRoom);
        }

        if (GUILayout.Button($"Unlock All Abilities"))
        {
            controller.DebugUnlockAbilities(toggleAbility);
            toggleAbility = !toggleAbility;
        }

        if (GUILayout.Button($"Switch Weather Condition"))
        {
            WeatherManager.instance.DebugSwitchCondition();
        }

        if (GUILayout.Button($"Sun Random Emote"))
        {
            UIEffectManager.Instance.GoodAction.Invoke();
        }

        if (GUILayout.Button($"Toggle Fog"))
        {
            WeatherManager.instance.DebugFogCondition();
        }

        if (GUILayout.Button($"Toggle Visual Count"))
        {
            UIManager.Instance.gameInterface.DebugToggleVisualCount(visualCountToggle);
            visualCountToggle = !visualCountToggle;
        }

        if (GUILayout.Button($"Enable Fever"))
        {
            ComboManager.Instance.StartFever();
        }

        if (GUILayout.Button($"Toggle Trailer Camera"))
        {
            CameraTargetController.instance.ToggleTrailerCamera(trailerCamToggle);
            FOVController.instance.trailerCamEnabled = trailerCamToggle;
            trailerCamToggle = !trailerCamToggle;
        }

        GUILayout.EndArea();
    }
}