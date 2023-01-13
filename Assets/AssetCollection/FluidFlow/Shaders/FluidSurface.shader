Shader "FluidFlow/Fluid (Surface Shader)"
{
	Properties
	{
		[Header(Main)]
		_Color("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex("Albedo (RGB)", 2D) = "white" {}
		[MaterialToggle] _DrawMain("Draw On Main", Float) = 0

		_Smoothness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		[NoScaleOffset] [Normal] _NormalTex("Normal", 2D) = "bump" {}
		[MaterialToggle] _DrawNormal("Draw On Normal", Float) = 0
		_NormalScale("Normal Scale", Float) = 1
		

		[Header(Fluid)] 
		_FluidScale("Fluid Scale", Float) = 1
		_FluidSmoothness("Fluid Smoothness", Range(0,1)) = 0.5
		_FluidMetallic("Fluid Metallic", Range(0,1)) = 0.0
		_FluidNormal("Fluid Normal Scale", Float) = 1
		// will be set automatically by the FFCanvas (if '_FluidTex' is specified as a texture channel)
		[HideInInspector] _FluidTex("Fluid", 2D) = "black" {}
		// needed to ensure fluid flow texture atlas is read properly (set automatically by FFCanvas component)
		[HideInInspector] _FF_AtlasTransform("Atlas Transform", Vector) = (0, 0, 1, 0)
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#include "UnityCG.cginc"
			
		// helper functions for writing custom fluid flow surface shaders
		#include "FFShaderUtil.cginc"

		#pragma target 3.0
		#pragma surface surf Standard fullforwardshadows vertex:vert
		// needed so fluid flow shader properties are initialized ^

		// needed when secondary uv is used for drawing/ fluid simulation (keyword set automatically by FFCanvas)
		#pragma multi_compile_local __ FF_UV1

		struct Input
		{
			float2 uv_MainTex;
			// macro adds default input variables, necessary for drawing on the model
			FF_SURFACE_INPUT
		};

		// default shader stuff..
		fixed4 _Color;
		sampler2D _MainTex;
		half _DrawMain;
		sampler2D _NormalTex;
		half _DrawNormal;
		half _NormalScale;
		half _Smoothness;
		half _Metallic;

		// properties defining the look of the fluid
		sampler2D _FluidTex;
		float4 _FluidTex_TexelSize;
		half _FluidScale;
		half _FluidSmoothness;
		half _FluidMetallic;
		half _FluidNormal;

		// needs to be declared above the vertex shader
		float4 _FF_AtlasTransform;
		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			// macro for initializing internal input variables
			FF_INITIALIZE_OUTPUT(v, o, _FF_AtlasTransform);

			// move vertices in normal direction, to simulate thickness of fluid
			//float d = min(2, tex2Dlod(_FluidTex, float4(o.FF_UV_NAME, 0, 0)).a) * .01;
            //v.vertex.xyz += v.normal * d;
		}

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			// when drawing on a texture channel, it has to be sampled with the atlas transformed uv (-> automatically calculated in IN.FF_UV_NAME)
			float2 mainUV = _DrawMain > 0 ? IN.FF_UV_NAME : IN.uv_MainTex;
			float2 normalUV = _DrawNormal > 0 ? IN.FF_UV_NAME : IN.uv_MainTex;

			// default shader stuff..
			fixed4 main = tex2D(_MainTex, mainUV) * _Color;
			float3 normal = UnpackNormal(tex2D(_NormalTex, normalUV));
			normal = normalize(float3(normal.xy * _NormalScale, normal.z));

			// sample fluid
			fixed4 fluid = tex2D(_FluidTex, IN.FF_UV_NAME);

			// approximate fluid normal
			float3 fluidNormal = FFUnpackFluidNormal(_FluidTex, _FluidTex_TexelSize.xy, IN.FF_UV_NAME, _FluidNormal);
			FF_TRANSFORM_NORMAL(IN, fluidNormal)

			// influence of fluid on the current pixel
			float fluidHeight = min(fluid.FF_FLUID_AMOUNT * _FluidScale, 1);

			// interpolate material properties depending on fluid influence on the current pixel
			o.Albedo = lerp(main, fluid.FF_FLUID_COLOR, fluidHeight);
			// partially keep underlying surface normal
			o.Normal = lerp(normal, fluidNormal, min(fluidHeight, .5f));
			o.Metallic = lerp(_Metallic, _FluidMetallic, fluidHeight);
			o.Smoothness = lerp(_Smoothness, _FluidSmoothness, fluidHeight);
		}
		ENDCG
	}
	FallBack "Diffuse"
}