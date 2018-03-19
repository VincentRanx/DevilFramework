// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DevilTeam/RefractionNormal-transport"
{
	Properties
	{
		_MainColor("Main Color", Color) = (1,1,1,0.5)
		_MainTex("Main Texture", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_Refraction("Refraction", Range(-1, 1)) = 0.1
		_Fresnel("Fresnel", Range(0, 3)) = 0.1
	}

	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Opaque" }
		LOD 100

		GrabPass {}

		Pass
		{
			
			Tags {"LightMode"="ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityLightingCommon.cginc" // for _LightColor0
			#include "UnityCG.cginc"

			struct v2f {
				float4 pos : SV_POSITION; // screen pos
				float4 uv : TEXCOORD0; // uv coord, xy for mainTex, zw for bump
				float3 worldPos : POSITION1; // world pos
				float3 tangent0 : TEXCOORD1; // tangent space matrix row0
				float3 tangent1 : TEXCOORD2; // tangent space matrix row1
				float3 tangent2 : TEXCOORD3; // tangent space matrix row2
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BumpMap;
			float4 _BumpMap_ST;

			v2f vert(appdata_tan v) {
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.uv.zw = TRANSFORM_TEX(v.texcoord, _BumpMap);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				
				half3 wNormal = UnityObjectToWorldNormal(v.normal);
				half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
				half tanSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 wBitangent = cross(wNormal, wTangent) * tanSign;
				o.tangent0 = half3(wTangent.x, wBitangent.x, wNormal.x);
				o.tangent1 = half3(wTangent.y, wBitangent.y, wNormal.y);
				o.tangent2 = half3(wTangent.z, wBitangent.z, wNormal.z);
				return o;
			}

			sampler2D _GrabTexture;
			half _Refraction;
			half4 _MainColor;
			half _Fresnel;

			half4 frag(v2f i) : SV_Target{

				half4 baseColor = tex2D(_MainTex, i.uv.xy) * _MainColor;// *i.diff;
				
				// unpack normal
				half3 normal = UnpackNormal(tex2D(_BumpMap, i.uv.zw));
				half3 worldNormal;
				worldNormal.x = dot(i.tangent0, normal);
				worldNormal.y = dot(i.tangent1, normal);
				worldNormal.z = dot(i.tangent2, normal);
				worldNormal = normalize(worldNormal);

				float3 viewVec = UnityWorldSpaceViewDir(i.worldPos);
				half3 viewDir = normalize(viewVec);
				half transparency = 1 - _MainColor.a;

				// calculate light color
				half3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
				half nl = max(0, dot(worldNormal, lightDir));
				half4 light = nl * _LightColor0;
				light.rgb += ShadeSH9(half4(worldNormal, 1));

				half fresnel = 1 - max(0, dot(viewDir, worldNormal));
				fresnel = max(0, fresnel - nl);
				light.rgb *= pow(1 + fresnel, _Fresnel);
				
				//// reflection
				//half3 worldRef = reflect(-viewDir, worldNormal);
				//half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, worldRef);
				//light.rgb *= (1 + saturate(DecodeHDR(skyData, unity_SpecCube0_HDR)));
				
				//light.rgb = lerp(light.rgb, half3(1, 1, 1), transparency);
				baseColor.rgb *= light;

				// refraction , TODO z test
				half f = length(viewVec);
				f = _Refraction * (f + _Refraction) / (f - _Refraction);
				float4 p = UnityWorldToClipPos(i.worldPos - float4(f * worldNormal, 0));
				half4 grabUV = ComputeGrabScreenPos(p);
				half4 refraction = tex2Dproj(_GrabTexture, grabUV);

				//blend
				half vdot = dot(viewDir, worldNormal);
				transparency *= (vdot * 0.5 + 0.5);
				half4 color;
				color.a = 1;
				color.rgb = baseColor.rgb * (1 - transparency) + refraction.rgb * transparency;
				return color;
			}

			ENDCG
		}
	
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}
