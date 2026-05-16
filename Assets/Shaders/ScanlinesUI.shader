Shader "RoguelikeTCG/ScanlinesUI"
{
    Properties
    {
        _Intensity  ("Intensity", Range(0, 1)) = 0.12
        _LineCount  ("Line Count (lines on screen)", Range(10, 500)) = 120
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex    : SV_POSITION;
                float4 screenPos : TEXCOORD0;
            };

            float _Intensity;
            float _LineCount;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // UV-space Y (0..1) — resolution-independent
                float uvY      = i.screenPos.y / i.screenPos.w;
                float phase    = fmod(uvY * _LineCount, 1.0);
                float scanline = step(phase, 0.5); // dark on first half, clear on second
                return fixed4(0.0, 0.0, 0.0, scanline * _Intensity);
            }
            ENDCG
        }
    }
}
