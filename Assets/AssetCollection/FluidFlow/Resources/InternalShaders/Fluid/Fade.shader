Shader "Hidden/FluidFlow/Fluid/Fade"
{
	Properties
	{
		[HideInInspector] _MainTex("Fluid", 2D) = "white" {}
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

			#pragma vertex blitvert
			#pragma fragment frag

			sampler2D_float _MainTex;
			float _FF_Amount;

			float4 frag(blit_v2f i) : SV_Target
			{
				float4 color = tex2D(_MainTex, i.uv);
				return float4(color.xyz, max(0, color.w - _FF_Amount));
			}
			ENDCG
		}
	}
}