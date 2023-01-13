Shader "Hidden/FluidFlow/Draw/Projection"
{
	SubShader
	{
		Cull Off
		Lighting Off
		ZWrite Off
		ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			// UV Set
			#pragma multi_compile_local __ FF_UV1

			// Color source
			#pragma multi_compile_local __ FF_COLOR

			// Mask (Optional)
			#pragma multi_compile_local __ FF_MASK

			// Decal Mode (Optional)
			#pragma multi_compile_local __ FF_FLUID FF_NORMAL

			#include "Draw.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			#if defined(FF_UV1)
				float2 uv : TEXCOORD1;
			#else
				float2 uv : TEXCOORD0;
			#endif

				float3 normal : NORMAL;

			#if defined(FF_NORMAL)
				float4 tangent : TANGENT;
			#endif

			#if defined(FF_NORMAL) && defined(FF_UV1)
				float4 transformation : FF_TANGENT_SPACE_TRANSFORMATION;
			#endif
			};

			struct v2f
			{
				float4 uv : SV_POSITION;
				float3 clip : TEXCOORD0;
				float2 texcoord : TEXCOORD1;
				float3 normal : TEXCOORD2;

			#if defined(FF_NORMAL)
				float4 tangent : TEXCOORD3;
			#endif
			};

			// projection
			float4x4 _FF_Projection;
			float _FF_PaintBackfacingSurface;

			// decal
			sampler2D _FF_DecalTex;
			// already defined in Draw.cginc
			// float4 _Color;
			// float _Data;

			// mask
			sampler2D _FF_MaskTex;
			float4 _FF_MaskComponents;
			float _FF_MaskComponentsInv;

			v2f vert(appdata v)
			{
				v2f o;
				o.texcoord = AtlasTransformUV(v.uv);
				o.uv = ProjectUVToClipSpace(o.texcoord);
				float3 vertexWorldSpace = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
				o.clip = ProjectToClipSpace(vertexWorldSpace, _FF_Projection);
				o.normal = UnityObjectToWorldNormal(v.normal);

			#if defined(FF_NORMAL)
				o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
			#endif
				return o;
			}


			float4 frag(v2f i) : SV_Target
			{
				// check if fragment is contained in projection frustum
				bool3 clip = i.clip > 0 && i.clip < 1;
				float factor = (clip.x && clip.y && clip.z) ? 1 : 0;
				float2 uv = float2(1 - i.clip.x, i.clip.y);

				// check if fragment is masked
			#if defined(FF_MASK)
				float mask = dot(tex2D(_FF_MaskTex, uv), _FF_MaskComponents) * _FF_MaskComponentsInv;
				factor *= mask;
			#endif

				// sample corresponding color
			#if defined(FF_COLOR)
				float4 colorSample = _FF_Color;
			#else
				float4 colorSample = tex2D(_FF_DecalTex, uv);
			#endif

			#if defined(FF_FLUID)
				// set fluid amount
				colorSample.w *= _FF_Data;
			#elif defined(FF_NORMAL)
				float3 normal = UnpackNormal(colorSample);
				float2 t = normalize(mul(_FF_Projection, float4(i.tangent.xyz, 0)).xy);
				normal.xy = mul(float2x2(-t.x, t.y, -t.y, -t.x), normal.xy) * _FF_Data;
				colorSample = PackNormal(normal);
			#endif
				
				// ensure normal of the surface points towards projector
				factor = (_FF_PaintBackfacingSurface || dot(i.normal, mul(float4(0, 0, 1, 0), _FF_Projection).xyz) < 0) ? factor : 0;

				return blend_draw(i.texcoord, colorSample, factor);
			}
			ENDCG
		}
	}
}