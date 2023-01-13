Shader "Hidden/FluidFlow/MarkOverlap"
{
	Properties
	{
		[HideInInspector] _MainTex("Main", 2D) = "black" {}
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
			#include "../../Resources/InternalShaders/FluidFlow.cginc"

			#pragma vertex blitvert
			#pragma fragment frag

			sampler2D _MainTex;

			float4 frag(blit_v2f i) : SV_Target
			{
				float4 pixel = tex2D(_MainTex, i.uv);
				if (pixel.x > .26f)
					return float4(pixel.x, 0, 0, 1);
				if (pixel.x > .0f)
					return float4(.3f, .3f, .3f, 1);
				return float4(0, 0, 0, 1);
			}
			ENDCG
		}
	}
}