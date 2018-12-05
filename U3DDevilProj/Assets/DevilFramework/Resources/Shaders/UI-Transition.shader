Shader "DevilTeam/UI-Transition"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

		_OffsetGray ("Gray Offset", Range(0, 1)) = 0.5
		[Toggle]_AlphaAsGray ("Alpha As Gray", Float) = 0
	}
	
	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "Default"
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			float _OffsetGray;
			float _AlphaAsGray;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = IN.texcoord;
				OUT.color = _Color * IN.color;
				return OUT;
			}

			sampler2D _MainTex;

			half mulOnFail(half v, half flag)
			{
				return flag + (1 - flag) * v;
			}
			
			half mulOnPass(half v, half flag)
			{
				return 1 - flag + flag * v;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				half4 col = IN.color;
				half4 color = (tex2D(_MainTex, IN.texcoord.xy) + _TextureSampleAdd);
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect) * mulOnFail(col.a, _AlphaAsGray);
				
				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				fixed gray = 0.2125 * color.r + 0.7154 * color.g + 0.0721 * color.b;
				fixed3 c0 = lerp(0, col.rgb, saturate(gray / _OffsetGray));
				fixed3 c1 = lerp(col.rgb, 1, saturate((gray - _OffsetGray) / (1 - _OffsetGray)));
				color.rgb = lerp(c0, c1, step(_OffsetGray, gray));
				color.rgb = lerp(fixed3(gray, gray, gray), color.rgb, mulOnPass(col.a, _AlphaAsGray));
				return color;
			}
		ENDCG
		}
	}
	Fallback "UI/Default"
}
