Shader "Skyrmion/BakeJDBK" {
        Properties{
            _MainTex("Base (RGB)", 2D) = "white" {}

            _Jmin("jmin", Float) = 0
            _Jmax("jmax", Float) = 1
            _Dmin("dmin", Float) = 0
            _Dmax("dmax", Float) = 1
            _Bmin("bmin", Float) = 0
            _Bmax("bmax", Float) = 1
            _Kmin("kmin", Float) = 0
            _Kmax("kmax", Float) = 1
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

        float4 _MainTex_ST;
        uniform sampler2D _MainTex;

        float _Jmin;
        float _Jmax;
        float _Dmin;
        float _Dmax;
        float _Bmin;
        float _Bmax;
        float _Kmin;
        float _Kmax;

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
            float J = tex2D(_MainTex, i.uv).r;
            float D = tex2D(_MainTex, i.uv).g;
            float B = tex2D(_MainTex, i.uv).b;
            float K = tex2D(_MainTex, i.uv).a;

            return float4(
                J * (_Jmax - _Jmin) + _Jmin,
                D * (_Dmax - _Dmin) + _Dmin,
                B * (_Bmax - _Bmin) + _Bmin,
                K * (_Kmax - _Kmin) + _Kmin
                //1.0
                );
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
