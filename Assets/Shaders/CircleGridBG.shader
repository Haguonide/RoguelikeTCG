Shader "RoguelikeTCG/CircleGridBG"
{
    Properties
    {
        _Scale       ("Circles Per Screen Height", Float)       = 8.0
        _Radius      ("Circle Radius",             Range(0.1, 0.9)) = 0.58
        _CircleColor ("Circle Color",              Color)        = (0.18, 0.48, 0.52, 1)
        _BGColor     ("Background Color",          Color)        = (0.10, 0.32, 0.36, 1)
        _OffsetX     ("Offset X",                  Float)        = 0
        _OffsetY     ("Offset Y",                  Float)        = 0
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue"="Background" "IgnoreProjector"="True" "RenderType"="Transparent" }
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

            float  _Scale, _Radius, _OffsetX, _OffsetY;
            float4 _CircleColor, _BGColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                float  aspect   = _ScreenParams.x / _ScreenParams.y;

                // Les cercles sont toujours ronds quelle que soit la résolution
                float2 uv   = float2(screenUV.x * aspect * _Scale + _OffsetX,
                                     screenUV.y           * _Scale + _OffsetY);

                float2 cell = frac(uv) - 0.5;
                float  dist = length(cell);

                // Anti-aliasing sur le bord des cercles
                float edge = 1.0 - smoothstep(_Radius - 0.02, _Radius + 0.02, dist);
                return lerp(_BGColor, _CircleColor, edge);
            }
            ENDCG
        }
    }
}
