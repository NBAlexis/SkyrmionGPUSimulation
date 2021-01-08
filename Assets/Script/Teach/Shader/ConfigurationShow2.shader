Shader "Skyrmion/ConfigurationShow2" {
    Properties{
        _Using("Using", 2D) = "white" {}

        [Toggle] _InverseNz("Inverse Nz", Float) = 0

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

        SubShader{
            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }

            Pass{

            Blend SrcAlpha OneMinusSrcAlpha
            Tags{ "RenderType" = "Transparent" }
            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            ColorMask[_ColorMask]

            //Fog { Mode off }
        BindChannels{
            Bind "Vertex", vertex
            Bind "TexCoord", texcoord
        }

        CGINCLUDE

            //#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
            //#pragma exclude_renderers molehill    
            //#pragma fragmentoption ARB_precision_hint_nicest

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            static const float PI = 3.14159265f;
            float4 _Using_ST;
            uniform sampler2D _Using;

            float4 _ClipRect;
            float _InverseNz;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };


            v2f vert(appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _Using);
                o.worldPosition = v.vertex;
                return o;
            }

            float4 frag(v2f i) : COLOR
            {
                float3 s = tex2D(_Using, i.uv).rgb;
                //s = (1.0f + s) * 0.5f;
                float fDarkness = saturate(1.0f - s.z * (1.0f - _InverseNz * 2.0f));

                float4 retC = saturate(
                    float4(
                        saturate(dot(float2(-1.0f, 1.0f), float2(s.x, s.y))) * fDarkness + (1.0f - fDarkness),
                        saturate(dot(float2(-1.0f, -1.0f), float2(s.x, s.y))) * fDarkness + (1.0f - fDarkness),
                        saturate(dot(float2(1.0f, 0.0f), float2(s.x, s.y))) * fDarkness + (1.0f - fDarkness),
                        1.0f
                    ));

#ifdef UNITY_UI_CLIP_RECT
                retC.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
#endif
                //retC = tex2D(_Using, i.uv) * 0.5 + 0.5;

#ifdef UNITY_UI_ALPHACLIP
                clip(retC.a - 0.001);
#endif
                return retC;

            }

        ENDCG

        CGPROGRAM

        #pragma vertex vert
        #pragma fragment frag
        #pragma fragmentoption ARB_precision_hint_nicest		


        ENDCG
        }

        }
}
