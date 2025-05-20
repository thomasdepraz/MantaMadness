using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RenderFeatureUtility
{
    public static List<ScriptableRendererFeature> GetRenderFeatures()
    {
        ScriptableRenderer renderer = (GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset).GetRenderer(0);
        var property = typeof(ScriptableRenderer).GetProperty("rendererFeatures", BindingFlags.NonPublic | BindingFlags.Instance);
        return property.GetValue(renderer) as List<ScriptableRendererFeature>;
    }

    public static ScriptableRendererFeature GetFeature(List<ScriptableRendererFeature> features, string name)
    {
        for (int i = 0; i < features.Count; i++)
        {
            if (features[i].name == name)
                return features[i];
        }

        return null;
    }
}
