Shader "RoguelikeTCG/RibbonArrow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _EdgePow ("Edge Power", Float) = 1.5
        _GlowStrength ("Glow Strength", Range(0,1)) = 0.25

        // Unity UI stencil / masking standard props
        _Stencil ("Stencil ID", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"  = "True"
            "RenderType"       = "Transparent"
            "PreviewType"      = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref   [_Stencil]
            Comp  [_StencilComp]
            Pass  [_StencilOp]
            ReadMask  [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "RibbonArrow"

            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4    _Color;
            float     _EdgePow;
            float     _GlowStrength;
            float4    _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.worldPos = v.vertex;
                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.uv       = v.texcoord;
                o.color    = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // u = 0 (queue) → 1 (pointe)   v = 0 (bord gauche) → 1 (bord droit)
                float u = i.uv.x;
                float v = i.uv.y;

                // Atténuation bords : maximum au centre (v=0.5)
                float edgeFactor = pow(4.0 * v * (1.0 - v), _EdgePow);

                // Fade à la queue de la flèche
                float tailFade = smoothstep(0.0, 0.12, u);

                float alpha = edgeFactor * tailFade * i.color.a;

                // Glow au centre du ruban
                float3 rgb = i.color.rgb * (1.0 + edgeFactor * _GlowStrength);

                // Clip UI masking
                alpha *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);

                return fixed4(rgb, alpha);
            }
            ENDCG
        }
    }
}
