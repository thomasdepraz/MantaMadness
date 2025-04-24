void Fog_float(float4 color, float4 fogColor, float depth, float farPlane, float density, float offset, out float4 finalColor)
{
    float viewDistance = depth * farPlane;
    float fogFactor = (density / sqrt(log(2))) * max(0.0f, viewDistance - offset);
    fogFactor = exp2(-fogFactor * fogFactor);
    
    finalColor = lerp(fogColor, color, saturate(fogFactor));
}