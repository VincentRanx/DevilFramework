Shader "DevilTeam/BlitImg"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
	}
	CGINCLUDE
#include "UnityCG.cginc"  

#define WEIGHT float3(0.147761, 0.118318, 0.0947416)
	struct v2f
	{
		float4 pos : SV_POSITION;   //顶点位置  
		float2 uv  : TEXCOORD0;     //纹理坐标 
		float2 uvOffset : TEXCOORD1;
	};

	//shader中用到的参数  
	sampler2D _MainTex;
	//XX_TexelSize，XX纹理的像素相关大小width，height对应纹理的分辨率，x = 1/width, y = 1/height, z = width, w = height  
	float4 _MainTex_TexelSize;
	//给一个offset，这个offset可以在外面设置，是我们设置横向和竖向blur的关键参数  
	float _BlurStep;
	float _BlurIters; // 迭代次数
	fixed4 _Color; // clear color

	//vertex shader  
	v2f vert_blur(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		//uv坐标  
		o.uv = v.texcoord.xy;

		o.uvOffset = _MainTex_TexelSize.xy * _BlurStep * _BlurIters;

		return o;
	}

	//fragment shader  
	fixed4 frag_blur(v2f i) : SV_Target
	{
		fixed4 color =
			WEIGHT.x * tex2D(_MainTex, i.uv) +
			WEIGHT.y * tex2D(_MainTex, i.uv + fixed2(i.uvOffset.x, 0)) +
			WEIGHT.y * tex2D(_MainTex, i.uv + fixed2(-i.uvOffset.x, 0)) +
			WEIGHT.y * tex2D(_MainTex, i.uv + fixed2(0, i.uvOffset.y)) +
			WEIGHT.y * tex2D(_MainTex, i.uv + fixed2(0, -i.uvOffset.y)) +
			WEIGHT.z * tex2D(_MainTex, i.uv + fixed2(i.uvOffset.x, i.uvOffset.y)) +
			WEIGHT.z * tex2D(_MainTex, i.uv + fixed2(i.uvOffset.x, -i.uvOffset.y)) +
			WEIGHT.z * tex2D(_MainTex, i.uv + fixed2(-i.uvOffset.x, i.uvOffset.y)) +
			WEIGHT.z * tex2D(_MainTex, i.uv + fixed2(-i.uvOffset.x, -i.uvOffset.y));
		//color.a = 1;
		return color;
	}

	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;
		o.uvOffset = 0;
		return o;
	}

	fixed4 clear_color(v2f i) : SV_Target
	{
		return _Color;
	}

	ENDCG

	//开始SubShader  
	SubShader
	{
		// blur pass
		Pass
		{
			Name "Blur"
			//后处理效果一般都是这几个状态  
			ZTest Off
			ZWrite Off
			Cull Off
			Blend One Zero

			//使用上面定义的vertex和fragment shader  
			CGPROGRAM
			#pragma vertex vert_blur  
			#pragma fragment frag_blur  
			ENDCG
		}

		// clear pass
		Pass
		{
			Name "Clear"
			ZTest Off
			ZWrite Off
			Cull Off
			Blend One Zero

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment clear_color
			ENDCG
		}
	}
}
