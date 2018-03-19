Shader "Hidden/Depth-tex"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Fact ("Fact ", Range(0,1)) = 1
		_ZScale ("ZScale", Float) = 1
		_CentDis("Center Position & Distance", Vector) = (0,0,0, 3)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 vPos : TEXCOORD1;
			};


			v2f vert (appdata v)
			{
				v2f o;
				o.vPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _CameraDepthTexture;
			float _Fact;
			float4 _CentDis;
			float _ZScale;

			fixed4 frag (v2f i) : SV_Target
			{
				float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.uv));
				float3 dir = normalize(i.vPos);
				float f = dot(dir, UNITY_MATRIX_V[2].xyz);
				float3 p = _WorldSpaceCameraPos + dir * depth / f;
				p.y = 0;
				float3 cent = _CentDis.xyz;
				cent.y = 0;
				float dis = _CentDis.w - length(cent - p) ;
				dis = (sign(dis) + 1) * 0.5;

				//fixed4 col = fixed4(depth, depth, depth, 1);
				fixed4 col = tex2D(_MainTex, i.uv);
				//col.rgb = depth * _Fact;
				col.a = 1;
				//dis = 1 - saturate(dis * dis / _CentDis.w);
				col.rgb *= dis;
				//col.rg = 0;
				//col.b = i.vPos.z * _Fact;
				return col;
			}
			ENDCG
		}
	}
}
