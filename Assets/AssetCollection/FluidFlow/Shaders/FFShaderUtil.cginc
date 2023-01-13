#ifndef FFSHADERUTIL_CG_INCLUDED
#define FFSHADERUTIL_CG_INCLUDED

// define the layout of the fluid textures
#define FF_FLUID_COLOR xyz
#define FF_FLUID_AMOUNT w


// default defines for a surface shader input struct
#define FF_UV_NAME FF_uv
#define FF_TRANSFORMATION_NAME FF_transformation

#define FF_SURFACE_INPUT_UV0 float2 FF_UV_NAME;
#define FF_SURFACE_INPUT_UV1 FF_SURFACE_INPUT_UV0 float4 FF_TRANSFORMATION_NAME;

#if defined(FF_UV1)
	#define FF_SURFACE_INPUT FF_SURFACE_INPUT_UV1
#else
	#define FF_SURFACE_INPUT FF_SURFACE_INPUT_UV0
#endif

// default defines for initializing a surface shader input struct
#define FF_TANGENT_SPACE_TRANSFORMATION texcoord2
#define FF_INITIALIZE_OUTPUT_UV0(appdata, output, atlasTransformation) {\
	(output).FF_UV_NAME = FFAtlasTransformUV((atlasTransformation), (appdata).texcoord.xy);}

#define FF_INITIALIZE_OUTPUT_UV1(appdata, output, atlasTransformation) {\
	(output).FF_UV_NAME = FFAtlasTransformUV((atlasTransformation), (appdata).texcoord1.xy);\
	(output).FF_TRANSFORMATION_NAME = (appdata).FF_TANGENT_SPACE_TRANSFORMATION;}

#if defined(FF_UV1)
	#define FF_INITIALIZE_OUTPUT FF_INITIALIZE_OUTPUT_UV1
#else
	#define FF_INITIALIZE_OUTPUT FF_INITIALIZE_OUTPUT_UV0
#endif

// convenience macro, for transforming a tangent space normal from uv1 to uv0, when necessary
#if defined(FF_UV1)
	#define FF_TRANSFORM_NORMAL(surface_input, normal) {\
		(normal).xyz = FFTransformNormalFromUV1(normal, (surface_input).FF_TRANSFORMATION_NAME);}
#else
	#define FF_TRANSFORM_NORMAL(surface_input, normal) {}
#endif

inline float2 FFAtlasTransformUV(in float4 transformation, in float2 uv) 
{
	return uv * transformation.z + transformation.xy;
}

inline float3 FFUnpackFluidNormal(in sampler2D fluidTex, in float2 texelSize, in float2 uv, in float scale)
{
	half l = tex2D(fluidTex, uv + float2(texelSize.x, 0)).FF_FLUID_AMOUNT;
	half r = tex2D(fluidTex, uv - float2(texelSize.x, 0)).FF_FLUID_AMOUNT;
	half u = tex2D(fluidTex, uv + float2(0, texelSize.y)).FF_FLUID_AMOUNT;
	half d = tex2D(fluidTex, uv - float2(0, texelSize.y)).FF_FLUID_AMOUNT;
	float2 gradient = float2(r - l, d - u) * scale;
	return normalize(float3(gradient, 1));;
}

inline float3 FFTransformNormalFromUV1(in float3 normal, in float4 texcoord2)
{
	normal.xy = mul(float2x2(texcoord2.xzyw), normal.xy);
	return normal;
}

#endif