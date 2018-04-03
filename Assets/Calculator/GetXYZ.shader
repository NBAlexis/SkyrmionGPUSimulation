// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Skyrmion/GetXYZ" {
	Properties {
		_MainTex    ("Magnetic momentum", 2D) = "white" {}

        _Nx("Nx", 2D) = "white" {}
        _Ny("Ny", 2D) = "white" {}
        _Nz("Nz", 2D) = "white" {}
	}

	SubShader {
        Tags{ "RenderType" = "Transparent" }

        Pass{

        Blend SrcAlpha OneMinusSrcAlpha
        Tags{ "RenderType" = "Transparent" }
        Lighting Off
        Cull Off
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

	    float4 _MainTex_ST;
        uniform sampler2D _MainTex;
        sampler2D_float _Nx;
        sampler2D_float _Ny;
        sampler2D_float _Nz;

	    struct v2f {
		    float4 pos : SV_POSITION;
		    float2 uv : TEXCOORD0;
	    };

	
	    v2f vert (appdata_full v)
	    {
		    v2f o;
		    o.pos = UnityObjectToClipPos(v.vertex);
		    o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		
		    return o;
	    }

        float4 frag(v2f i) : COLOR
        {
            return 0.5f * float4(tex2D(_Nx, i.uv).r, tex2D(_Ny, i.uv).r, tex2D(_Nz, i.uv).r, 1.0f) + 0.5f;
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