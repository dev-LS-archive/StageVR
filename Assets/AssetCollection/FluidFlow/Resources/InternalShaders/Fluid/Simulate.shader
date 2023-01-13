Shader "Hidden/FluidFlow/Fluid/Simulate"
{
	Properties
	{
		[HideInInspector] _MainTex("Fluid", 2D) = "black" {}
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

			#pragma multi_compile_local __ FF_FLOWTEX_COMPRESSED

			sampler2D _MainTex;
			sampler2D_float _FF_FlowTex;
			float4 _FF_FlowTex_TexelSize;
			float2 _FF_FluidRetained;
			float _FF_FluidRetainedInv;

			inline float2 calcFlow(in float2 uv, in float fluid, in bool sampleOther)
			{
				float4 flow = tex2D(_FF_FlowTex, uv);
				if (!sampleOther && IsZero(flow)) // early out for performance and stability
					discard;
				float2 gravity = UnpackGravity(flow);
				float retained = UnpackRetainedFluid(flow);
				bool seam = IsSeam(flow);
				if (seam && sampleOther) {
					// uv position of other uv island
					uv = UnpackSeamUV(flow, _FF_FlowTex_TexelSize);
					// sample gravity from seam
					float4 otherFlow = tex2D(_FF_FlowTex, uv);
					seam = IsSeam(otherFlow); // ensure sampled proper position
					// rotate gravity to current uv island
					gravity = mul(UnpackGravity(otherFlow), UnpackSeamRotation(flow));
					retained = UnpackRetainedFluid(otherFlow);
				}
				float flowingAmount = max(0, fluid - lerp(_FF_FluidRetained.x, _FF_FluidRetained.y, retained));
				float2 flowing = gravity * gravity * sign(gravity) * flowingAmount;
				return flowing * (!seam);
			}

			static const float epsilon = .0001f;
			static const float max_fluid = 1000;

			float4 frag(blit_v2f i) : SV_Target
			{
				float2 uvs[4] = { 
					i.uv - float2(1, 0) * _FF_FlowTex_TexelSize.xy, 
					i.uv + float2(1, 0) * _FF_FlowTex_TexelSize.xy, 
					i.uv + float2(0, 1) * _FF_FlowTex_TexelSize.xy, 
					i.uv - float2(0, 1) * _FF_FlowTex_TexelSize.xy 
				};

				// sample fluid
				float4 texel = tex2D(_MainTex, i.uv);
				float4 texels[4] = {
					tex2D(_MainTex, uvs[0]),
					tex2D(_MainTex, uvs[1]),
					tex2D(_MainTex, uvs[2]),
					tex2D(_MainTex, uvs[3])
				};

				// sample flow
				float2 outflow = abs(calcFlow(i.uv, texel.w, false));
				float4 inflow = float4(
					max(0, -calcFlow(uvs[0], texels[0].w, true).x),
					max(0, calcFlow(uvs[1], texels[1].w, true).x),
					max(0, calcFlow(uvs[2], texels[2].w, true).y),
					max(0, -calcFlow(uvs[3], texels[3].w, true).y)
				);

				// fluid flowing into this texel
				float totalInflow = inflow.x + inflow.y + inflow.z + inflow.w;

				// weighted average color of the fluid flowing into this texel 
				inflow += (totalInflow == 0) * (1.0f / 4);
				float3 inflowColor = (
					texels[0].xyz * inflow.x + 
					texels[1].xyz * inflow.y + 
					texels[2].xyz * inflow.z + 
					texels[3].xyz * inflow.w) / (totalInflow > epsilon ? totalInflow : 1);

				// calculate new color and fluid amount of this texel
				//texel.xyz = (totalInflow > 0) || (texel.w == 0) ?  inflowColor : texel.xyz;
				texel.xyz = lerp(texel.xyz, inflowColor, clamp(totalInflow * _FF_FluidRetainedInv + (texel.w == 0 ? 1 : 0), 0, 1));
				texel.w = min(max(0, texel.w - outflow.x - outflow.y) + totalInflow, max_fluid);
				return texel;
			}
			ENDCG
		}
	}
}