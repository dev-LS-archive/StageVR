Shader "Hidden/FluidFlow/Fluid/UVSeamPadding"
{
	Properties
	{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM
			#include "../FluidFlow.cginc"
			#include "../Padding.cginc"

			#pragma vertex blitvert
			#pragma fragment frag

			sampler2D_float _MainTex;
			float4 _MainTex_TexelSize;

			float4 frag (blit_v2f i) : SV_Target
			{
				return PadFlowTexture(_MainTex, _MainTex_TexelSize, i.uv);
			}
			ENDCG
		}
	}
}