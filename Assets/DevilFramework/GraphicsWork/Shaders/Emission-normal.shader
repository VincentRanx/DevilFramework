Shader "DevilTeam/Emission-Normal" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_BumpMap("Normal Map", 2D) = "bump" {}

		_RimColor("Rim Color", Color) = (1,1,1,1)
		_RimPower("Rim Power", Range(0, 8.0)) = 1
	}
	SubShader {
		Tags { "RenderType"="Qpaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		#include "UnityCG.cginc"

#define BOARDER_EMISSION

		sampler2D _MainTex;
		sampler2D _BumpMap;
		float4 _RimColor;
		float _RimPower;


		struct Input {
			float2 uv_MainTex;
			float3 viewDir;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf(Input IN, inout SurfaceOutputStandard o) 
		{
			fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = color.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
#ifdef BOARDER_EMISSION
			half rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
#else
			half rim = saturate(dot(normalize(IN.viewDir), o.Normal));
			rim *= rim;
#endif
			o.Emission = _RimColor *  saturate(rim * rim * _RimPower);
			o.Alpha = _Color.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
