// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Skyrmion/LLG" {
	Properties {
		_MainTex    ("Magnetic momentum", 2D) = "white" {}
        _params1    ("width, height, lx, ly", Vector) = (1024.0, 1024.0, 0.01, 0.01)
        _params2    ("time step, electric current, gamma, alpha",    Vector) = (0.01, 0.0, 1.0, 0.01)
        _params3    ("J, D, B, K",            Vector) = (1.0, 0.1, 0.05, 0.01)
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

        #define PARAM_INV_WIDTH (1.0f / _params1.x)
        #define PARAM_INV_HEIGHT (1.0f / _params1.y)
        #define PARAM_TIME_STEP (_params2.x)
        #define PARAM_J (_params3.x)
        #define PARAM_D (_params3.y)
        #define PARAM_B (_params3.z)
        #define PARAM_ELECTRIC (_params2.y)
        #define PARAM_ALPHA (_params2.w)

        static const float PI = 3.14159265f;
	    float4 _MainTex_ST;
        uniform sampler2D _MainTex;
        float4 _params1;
        float4 _params2;
        float4 _params3;

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

        //!!!!!!!!!!!!!!!!
        //Only Red And Blue(most blue)
        //Which means, Sx > 0, Sy > 0 all the time!!!
        float4 frag(v2f i) : COLOR
        {
            float3 s = 2.0f * (tex2D(_MainTex, i.uv).rgb - 0.5f);

            float3 sleft = 2.0f * (tex2D(_MainTex, saturate(i.uv - float2(PARAM_INV_WIDTH, 0.0f))).rgb - 0.5f);

            float3 sright = 2.0f * (tex2D(_MainTex, saturate(i.uv + float2(PARAM_INV_WIDTH, 0.0f))).rgb - 0.5f);
            float3 vright = float3(1.0, 0.0, 0.0);

            float3 sup = 2.0f * (tex2D(_MainTex, saturate(i.uv - float2(0.0f, PARAM_INV_HEIGHT))).rgb - 0.5f);

            float3 sdown = 2.0f * (tex2D(_MainTex, saturate(i.uv + float2(0.0f, PARAM_INV_HEIGHT))).rgb - 0.5f);
            float3 vdown = float3(0.0, 1.0, 0.0);
            
            float3 heff = PARAM_J * (sleft + sright + sup + sdown)
                + PARAM_D * (cross(sright, vright) - cross(sleft, vright) + cross(sdown, vdown) - cross(sup, vdown))
                + float3(0.0f, 0.0f, PARAM_B);

            //spin transfer torque
            float3 stt = -PARAM_ELECTRIC * cross(s, cross((sright - sleft) * 0.5f, s));

            float3 newS = cross(s, heff) + stt;
            newS = newS - PARAM_ALPHA * cross(s, newS);
            float3 retC = normalize(s + PARAM_TIME_STEP * newS);
            retC = retC * 0.5f + 0.5f;

            return saturate(
                float4( 
                    retC.r,
                    retC.g,
                    retC.b,
                    1.0f
                ));
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