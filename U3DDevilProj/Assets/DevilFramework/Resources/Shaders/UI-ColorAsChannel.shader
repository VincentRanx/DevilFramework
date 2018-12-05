/*
color channels: r:饱和度, g:剪影程度, b:对比度
*/

Shader "DevilTeam/UI-ColorAsChannel"
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
			#pragma multi_compile __ UNITY_UI_ALPHACLIP
			
		ENDCG
		}
	}
	
CGINCLUDE
		
		#include "UnityCG.cginc"
		#include "UnityUI.cginc"
		
		#define GRAY(color) dot(color.rgb, half3(0.375, 0.3125, 0.3125))
		
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
			float3 channels : TEXCOORD2;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		fixed4 _Color;
		fixed4 _TextureSampleAdd;
		float4 _ClipRect;

		v2f vert(appdata_t IN)
		{
			v2f OUT;
			UNITY_SETUP_INSTANCE_ID(IN);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
			OUT.worldPosition = IN.vertex;
			OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
			//half sat = dot(IN.color.rgb, half3(0.3125, 0.3125, 0.375));
			OUT.texcoord = IN.texcoord;
			fixed4 color = _Color;
			color.a *= IN.color.a;
			OUT.color = color;
			OUT.channels = IN.color.rgb;
			return OUT;
		}

		sampler2D _MainTex;

		fixed4 frag(v2f IN) : SV_Target
		{
			half4 color = (tex2D(_MainTex, IN.texcoord.xy) + _TextureSampleAdd) * IN.color;
				
			color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				
			#ifdef UNITY_UI_ALPHACLIP
			clip (color.a - 0.001);
			#endif

			fixed gray = GRAY(color); //dot(GRAY_DOT, color.rgb);
			color.rgb = lerp(fixed3(gray, gray, gray), color.rgb, IN.channels.r);

			return color;
		}
ENDCG

}
