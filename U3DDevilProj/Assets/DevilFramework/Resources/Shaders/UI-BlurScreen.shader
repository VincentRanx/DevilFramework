Shader "DevilTeam/UI-BlurScreen"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

		_BlurStep("Blur Step", Float) = 1
	}

		CGINCLUDE

		#include "UnityCG.cginc"
		#include "UnityUI.cginc"
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
			float4 worldPosition : POSITION1;
			fixed4 color : COLOR;
			float2 texcoord  : TEXCOORD0;
			float4 grabPos : TEXCOORD1;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		struct dev2f
		{
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float4 uv01 : TEXCOORD0;
			float4 uv23 : TEXCOORD1;
		};

		sampler2D _MainTex;
		fixed4 _Color;
		fixed4 _TextureSampleAdd;
		float4 _ClipRect;
		sampler2D _GrabTexture;
		float4 _GrabTexture_TexelSize;
		half _BlurStep;
// weight for blend matrix
#define WEIGHT float3(0.147761, 0.118318, 0.0947416)

		v2f vert(appdata_t IN)
		{
			v2f OUT;
			UNITY_SETUP_INSTANCE_ID(IN);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
			OUT.worldPosition = IN.vertex;
			OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
			OUT.texcoord = IN.texcoord;
			OUT.color = _Color * IN.color;
			OUT.grabPos = ComputeGrabScreenPos(OUT.vertex);
			return OUT;
		}

		// step: (x,y,0)
		fixed4 texGrab(fixed4 pos, fixed2 step)
		{
			return tex2Dproj(_GrabTexture, pos) * WEIGHT.x +
				WEIGHT.y * tex2Dproj(_GrabTexture, pos + float4(step.x, 0, 0, 0)) +
				WEIGHT.y * tex2Dproj(_GrabTexture, pos + float4(-step.x, 0, 0, 0)) +
				WEIGHT.y * tex2Dproj(_GrabTexture, pos + float4(0, step.y, 0, 0)) +
				WEIGHT.y * tex2Dproj(_GrabTexture, pos + float4(0, -step.y, 0, 0)) +
				WEIGHT.z * tex2Dproj(_GrabTexture, pos + float4(step.x, step.y, 0, 0)) +
				WEIGHT.z * tex2Dproj(_GrabTexture, pos + float4(step.x, -step.y, 0, 0)) +
				WEIGHT.z * tex2Dproj(_GrabTexture, pos + float4(-step.x, step.y, 0, 0)) +
				WEIGHT.z * tex2Dproj(_GrabTexture, pos + float4(-step.x, -step.y, 0, 0));
		}

		fixed4 blur1(v2f IN) : SV_Target
		{
			#ifdef UNITY_UI_ALPHACLIP
			clip(UnityGet2DClipping(IN.worldPosition.xy, _ClipRect) * IN.color.a - 0.001);
			#endif
			half2 step = _GrabTexture_TexelSize.xy * _BlurStep * IN.color.a;
			return texGrab(IN.grabPos, step);
		}

		fixed4 blur2(v2f IN) : SV_Target
		{
			#ifdef UNITY_UI_ALPHACLIP
			clip(UnityGet2DClipping(IN.worldPosition.xy, _ClipRect) - 0.001);
			#endif
			half2 step = _GrabTexture_TexelSize.xy * _BlurStep * IN.color.a * 2;
			return texGrab(IN.grabPos, step);
		}

		fixed4 blur3(v2f IN) : SV_Target
		{
			#ifdef UNITY_UI_ALPHACLIP
			clip(UnityGet2DClipping(IN.worldPosition.xy, _ClipRect) - 0.001);
			#endif
			half2 step = _GrabTexture_TexelSize.xy * _BlurStep * IN.color.a * 3;
			return texGrab(IN.grabPos, step);
		}

		fixed4 finalBlur(v2f IN) : SV_Target
		{
			half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
			color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

			half2 step = _GrabTexture_TexelSize.xy * _BlurStep * IN.color.a * 4;
			half4 col = texGrab(IN.grabPos, step);
			//color.rgb *= col.rgb;
			col.rgb = lerp(color.rgb, col.rgb, color.a);
			color.rgb *= col.rgb;

			#ifdef UNITY_UI_ALPHACLIP
			clip(color.a - 0.001);
			#endif
			return color;
		}

		fixed4 frag(v2f IN) : SV_Target
		{
			half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

			color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect) * 0.5;

			#ifdef UNITY_UI_ALPHACLIP
			clip(color.a - 0.001);
			#endif

			return color;
		}
		ENDCG

		SubShader
		{
			LOD 500
				Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

				Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

				Cull Off
				Lighting Off
				ZWrite Off
				ZTest[unity_GUIZTestMode]
				Blend SrcAlpha OneMinusSrcAlpha
				ColorMask[_ColorMask]

				GrabPass{}
				Pass
			{
				Name "Blur1"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment blur1
				#pragma target 2.0

				#pragma multi_compile __ UNITY_UI_ALPHACLIP

				ENDCG
			}
				GrabPass{}
				Pass
			{
				Name "Blur11"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment blur2
				#pragma target 2.0

				#pragma multi_compile __ UNITY_UI_ALPHACLIP

				ENDCG
			}

				GrabPass{}
				Pass
			{
				Name "Blur2"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment blur3
				#pragma target 2.0

				#pragma multi_compile __ UNITY_UI_ALPHACLIP

				ENDCG
			}
				GrabPass{}
				Pass
			{
				Name "Blur3"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment blur3
				#pragma target 2.0

				#pragma multi_compile __ UNITY_UI_ALPHACLIP

				ENDCG
			}

				GrabPass{}
				Pass
			{
				Name "FinalBlur"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment finalBlur
				#pragma target 2.0
				#pragma multi_compile __ UNITY_UI_ALPHACLIP
				ENDCG
			}
		}

				SubShader
			{
				LOD 300
					Tags
				{
					"Queue" = "Transparent"
					"IgnoreProjector" = "True"
					"RenderType" = "Transparent"
					"PreviewType" = "Plane"
					"CanUseSpriteAtlas" = "True"
				}

					Stencil
				{
					Ref[_Stencil]
					Comp[_StencilComp]
					Pass[_StencilOp]
					ReadMask[_StencilReadMask]
					WriteMask[_StencilWriteMask]
				}

					Cull Off
					Lighting Off
					ZWrite Off
					ZTest[unity_GUIZTestMode]
					Blend SrcAlpha OneMinusSrcAlpha
					ColorMask[_ColorMask]

					GrabPass{}
					Pass
				{
					Name "Blur1"
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment blur1
					#pragma target 2.0

					#pragma multi_compile __ UNITY_UI_ALPHACLIP

					ENDCG
				}
				
					GrabPass{}
					Pass
				{
					Name "Blur2"
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment blur2
					#pragma target 2.0

					#pragma multi_compile __ UNITY_UI_ALPHACLIP

					ENDCG
				}
				
					GrabPass{}
					Pass
				{
					Name "FinalBlur"
					CGPROGRAM
					#pragma vertex vert
					#pragma fragment finalBlur
					#pragma target 2.0
					#pragma multi_compile __ UNITY_UI_ALPHACLIP
					ENDCG
				}
			}

		SubShader
		{
			LOD 100

			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			GrabPass{}

			Pass
			{
				Name "Blur1"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment blur2
				#pragma target 2.0

				#pragma multi_compile __ UNITY_UI_ALPHACLIP

				ENDCG
			}

			GrabPass{}

			Pass
			{
				Name "FinalBlur"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment finalBlur
				#pragma target 2.0
				#pragma multi_compile __ UNITY_UI_ALPHACLIP
				ENDCG
			}
		}

			SubShader
		{
			LOD 0

			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}

			Cull Off
			Lighting Off
			ZWrite Off
			ZTest[unity_GUIZTestMode]
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask[_ColorMask]

			GrabPass{}
			Pass
			{
				Name "Default"
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment finalBlur
				#pragma target 2.0
				#pragma multi_compile __ UNITY_UI_ALPHACLIP
				ENDCG
			}
		}

			Fallback "UI/Default"
}
