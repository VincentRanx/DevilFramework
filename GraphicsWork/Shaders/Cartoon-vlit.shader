Shader "DevilTeam/Cartoon-Vlit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Outline ("Outline", Range(0, 0.5)) = 0.3
		_LightGradient("Light Gradient", Range(0.1,0.5)) = 0.3
	}

	CGINCLUDE
	// 重新分布光照
	fixed remapLight(fixed3 light, fixed gradient) 
	{
		light.rgb *= light.rgb;
		fixed3 color = (1 + floor(light / gradient)) * gradient;
		return saturate(color);
	}
	ENDCG

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{

			Tags{ "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#include "AutoLight.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1) // put shadows data into TEXCOORD1
				fixed3 diff : COLOR0;
				fixed3 ambient : COLOR1;
				float4 pos : SV_POSITION;
				fixed3 normal : NORMAL;
				fixed3 worldPos : POSITION1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				o.normal = worldNormal;
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0.rgb;
				o.ambient = ShadeSH9(half4(worldNormal,1));
				TRANSFER_SHADOW(o)
				return o;
			}

			fixed _Outline;
			fixed _LightGradient;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				if (dot(viewDir, i.normal) < _Outline)
				{
					col.rgb *= 0;
				}
				else
				{
					fixed shadow = SHADOW_ATTENUATION(i);
					fixed3 lighting = i.diff * shadow + i.ambient;
					col.rgb *= remapLight(lighting, _LightGradient);
				}
				return col;
			}
			ENDCG
		}

		Pass
		{
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
