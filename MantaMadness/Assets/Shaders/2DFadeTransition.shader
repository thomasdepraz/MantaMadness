Shader "Custom/2DTextureProjectorSprite_WORKING"
{
Properties
{
[NoScaleOffset]_Texture("Texture", 2D) = "white" {}
[HDR]_Color("Color", Color) = (1,1,1,1)
_Tiling("Tiling", Vector) = (1,1,0,0)
_SpeedDirection("SpeedDirection", Vector) = (1,1,0,0)
_Alpha("Alpha", Range(0,1)) = 1

    // UI MASK SUPPORT
    _Stencil ("Stencil ID", Float) = 0
    _StencilComp ("Stencil Comparison", Float) = 8
    _StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("Stencil Read Mask", Float) = 255
    _ColorMask("ColorMask", Float) = 15
}

SubShader
{
    Tags
    {
        "Queue"="Transparent"
        "RenderType"="Transparent"
        "IgnoreProjector"="True"
    }

    Pass
    {
        Name "UI"
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        HLSLPROGRAM
        #pragma vertex vert
        #pragma fragment frag

        #include "UnityCG.cginc"

        struct appdata_t
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float4 screenPos : TEXCOORD0;
            float4 color : COLOR;
        };

        sampler2D _Texture;
        float4 _Color;
        float2 _Tiling;
        float2 _SpeedDirection;
        float _Alpha;

        v2f vert (appdata_t v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.screenPos = ComputeScreenPos(o.vertex);
            o.color = v.color;
            return o;
        }

        fixed4 frag (v2f i) : SV_Target
        {
            float2 uv = i.screenPos.xy / i.screenPos.w;

            // scroll + tiling
            uv *= _Tiling;
            uv += _Time.y * _SpeedDirection;

            fixed4 tex = tex2D(_Texture, uv);

            fixed4 col = tex * _Color * i.color;

            col.a = saturate(col.a * _Alpha);

            return col;
        }

        ENDHLSL
    }
}

}
