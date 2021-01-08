Shader "Skyrmion/NtoDegree" {
    Properties{
        _MainTex("Base (RGB)", 2D) = "white" {}
    }

        SubShader{

            Pass{

            Blend SrcAlpha OneMinusSrcAlpha
            Tags{ "RenderType" = "Transparent" }
            Cull Off
            Lighting Off
            ZWrite Off


        CGINCLUDE

        //#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
        //#pragma exclude_renderers molehill    
        //#pragma fragmentoption ARB_precision_hint_nicest

        #include "UnityCG.cginc"
        #include "UnityUI.cginc"

        #pragma multi_compile __ UNITY_UI_CLIP_RECT
        #pragma multi_compile __ UNITY_UI_ALPHACLIP
        static const float PI = 3.14159265f;
        float4 _MainTex_ST;
        sampler2D _MainTex;

        struct v2f {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 worldPosition : TEXCOORD1;
        };


        v2f vert(appdata_full v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.worldPosition = v.vertex;
            return o;
        }

        float4 frag(v2f i) : COLOR
        {
            return tex2D(_MainTex, i.uv) * 0.5 + 0.5;
            //float3 nxyz = tex2D(_MainTex, i.uv).xyz;

            //float fTheta = acos(clamp(nxyz.z, -1, 1));
            //if (fTheta < 0)
            //{
            //    fTheta = fTheta + 2 * PI;
            //}
            //if (fTheta > 2 * PI)
            //{
            //    fTheta = fTheta - 2 * PI;
            //}
            //if (fTheta > PI)
            //{
            //    fTheta = 2 * PI - fTheta;
            //}

            //float fPhi = atan2(nxyz.y, nxyz.x);
            //if (fPhi < 0)
            //{
            //    fPhi = fPhi + 2 * PI;
            //}

            //return float4(saturate(fTheta / PI), saturate(0.5 * fPhi / PI), 0.0, 1.0);
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
