// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Skyrmion/GetXYZ" {
	Properties {
		_MainTex    ("Magnetic momentum", 2D) = "white" {}
        _Filter     ("Filter", Vector) = (1, 0, 0, 0)
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
	    #include "UnityCG.cginc"

	    float4 _MainTex_ST;
        uniform sampler2D _MainTex;
        float4 _Filter;

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

        float frag(v2f i) : COLOR
        {
            float3 s = 2.0f * (tex2D(_MainTex, i.uv).rgb - 0.5f);
            return dot(s, _Filter.xyz);
        }

	    ENDCG

	    CGPROGRAM

	    #pragma vertex vert
	    #pragma fragment frag
	    #pragma fragmentoption ARB_precision_hint_fastest		


	    ENDCG
	    }
	
	}
}