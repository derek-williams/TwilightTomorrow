Shader "Vignette" {

	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_Density ("Density", Range(0.0,1.0)) = 0.25
		_Fade ("Fade", Range(0.0,1.0)) = 0.0
		}
	
	SubShader {
		Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		LOD 500
		
		CGPROGRAM
		#pragma surface surf Lambert alpha
		
		struct Input 
		{
			float2 uv_MainTex;
		};
		sampler2D _MainTex;
		float _Density;
		float _Fade;
		
		void surf (Input IN, inout SurfaceOutput o) 
		{
		    half2 uv = IN.uv_MainTex.xy;
		    o.Alpha = clamp(_Fade + _Density * (1.0 - clamp( 0.3 + 0.5*64.0*uv.x*uv.y*(1.0-uv.x)*(1.0-uv.y), 0.0, 1.0)), 0.0, 1.0);
		    
			/*
			half2 vigUV = abs((IN.uv_MainTex.xy - 0.5f) * 2.0f);
			vigUV *= vigUV;
			half vig = vigUV.x + vigUV.y;
		
			//o.Albedo = c.rgb;
			o.Alpha = lerp(0.0, vig, _Density);
			*/
		}
		ENDCG
	}

Fallback "Transparent/VertexLit"
}


//Shader "BDash/Vignette" {
//	
//	Properties {
//				_Color ("Main Color", Color) = (1,1,1,1)
//			}
//	
//	SubShader {
//	
//		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
//		LOD 200
//	    Pass {
//	
//			CGPROGRAM
//			#pragma vertex vert
//			#pragma fragment frag
//			//#include "UnityCG.cginc"
//			
//			// vertex input: position, UV
//			struct appdata {
//			    float4 vertex : POSITION;
//			    float4 texcoord : TEXCOORD0;
//			};
//			
//			struct v2f {
//			    float4 pos : SV_POSITION;
//			   // float3 color : COLOR0;
//			    float2 uv : TEXCOORD0;
//				};
//			
//			
//			v2f vert (float4 vertex : POSITION, float2 uv : TEXCOORD0)
//			{
//			    v2f o;
//			    o.pos = mul (UNITY_MATRIX_MVP, vertex);
//			    o.uv = uv;
//			    return o;
//			}
//			
//			half4 frag (v2f i) : COLOR
//			{
//				half2 vigUV = abs((i.uv.xy - 0.5f) * 2.0f);
//				vigUV *= vigUV;
//				half vig = vigUV.x + vigUV.y;
//			
//			  //  return half4 (0,0,0, vig);
//			    
//			    return half4(vigUV.xxy, 0.5);
//			}
//			ENDCG
//	
//	    }
//	}
//	Fallback "Transparent/VertexLit"
//}
