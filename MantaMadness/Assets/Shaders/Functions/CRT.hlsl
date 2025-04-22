void Warp_float(float2 originUV, float curvature, out float2 warpedUV)
{
    //Offset
    warpedUV = originUV * 2.0f - 1.0f;
    float2 offset = warpedUV.yx / curvature;

    //Curve
    warpedUV = warpedUV + warpedUV * offset * offset;
    
    //Center
    warpedUV = warpedUV * 0.5f + 0.5f;
}

void Filter_float(float4 color, float2 warpedUV, float vignetteWidth, float2 screen, out float4 filteredColor)
{
    //filter color to black outside of warp
    filteredColor = color;
    if (warpedUV.x <= 0.0f || 1.0f <= warpedUV.x || warpedUV.y <= 0.0f || 1.0f <= warpedUV.y )
    {
        filteredColor = 0.0f;
    }
    
    //apply vignette
    warpedUV = warpedUV * 2.0f - 1.0f;
    float2 vignette = vignetteWidth / screen.xy;
    vignette = smoothstep(0.0f, vignette, 1.0f - abs(warpedUV));
    vignette = saturate(vignette);
    
    //offset colors
    
    float t = warpedUV.y * screen.y * 2.0f;
    filteredColor.r *= (sin(t) + 1.0f) * 0.15f + 1.0f;
    filteredColor.rb *= (cos(t) + 1.0f) * 0.135f + 1.0f;
    
    filteredColor = saturate(filteredColor) * vignette.x * vignette.y;
}