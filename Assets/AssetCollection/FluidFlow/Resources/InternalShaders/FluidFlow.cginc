#ifndef FLUIDFLOW_CG_INCLUDED
#define FLUIDFLOW_CG_INCLUDED

#include "UnityStandardUtils.cginc"

#define PI (3.14159265358979323846)
#define PI_INV (1.0 / PI)
#define PI2 (PI * 2.0)
#define PI2_INV (1.0 / PI2)

#define FF_TANGENT_SPACE_TRANSFORMATION TEXCOORD2

float2 _FF_FlowTexture_Packing;
float4 _FF_AtlasTransform;

inline float2 AtlasTransformUV(in float2 uv)
{
	return uv * _FF_AtlasTransform.z + _FF_AtlasTransform.xy;
}

inline float4 ProjectUVToClipSpace(in float2 uv)
{
#if defined(UNITY_UV_STARTS_AT_TOP)
	return float4(uv.x * 2 - 1, 1 - uv.y * 2, 0, 1);
#else
	return float4(uv.x * 2 - 1, uv.y * 2 - 1, 0, 1);
#endif
}

inline float3 ProjectToClipSpace(in float3 pos, in float4x4 projection)
{
	float4 projected = mul(projection, float4(pos, 1));
	return ((projected.xyz / projected.w) + 1) * .5f;
}

inline float VectorToMask(in float4 vec, in float4 maskChannel, in float4 maskInfo)
{
	float mask = dot(vec, maskChannel) * maskInfo.x;
	return mask <= maskInfo.y ? 0 : clamp(0, 1, mask + maskInfo.z);
}

inline float2x2 AngleToRotation(in float a) 
{
	float s = sin(a);
	float c = cos(a);
	return float2x2(c, -s, s, c);
}

inline float2 PolarToDirection(in float a) 
{
	return float2(cos(a), sin(a));
}

inline float2 SnapDirection(in float2 direction) 
{
	return abs(direction.x) > abs(direction.y) 
		? float2(sign(direction.x), 0) 
		: float2(0, sign(direction.y));
}

inline float3x3 CreateTangentToWorld(in float3 normal, in float4 tangent)
{
	float3 binormal = cross(normal, tangent.xyz) * tangent.w; // * unity_WorldTransformParams.w;	// TODO: not updated properly in build
	return float3x3(tangent.xyz, binormal, normal);
}

inline float2 SnapToTexel(in float2 uv, in float4 texelSize)
{
	return (floor(uv * texelSize.zw) + .5f) * texelSize.xy;
}

inline float2 ClampLength(in float2 vec, in float maxLength)
{
	float len = length(vec);
	return vec * (min(maxLength, len) / (len == 0 ? 1 : len));
}

inline float4 PackNormal(in float3 normal)
{
	normal = normalize(normal);
#if defined(UNITY_NO_DXT5nm)
	return float4(normal * .5 + .5, 1);
#else
	return float4(1, normal.y * .5 + .5, 1, normal.x * .5 + .5);
#endif
}

inline float4 PackFlowGravity(in float3 grav) 
{
	return float4(grav.xy * (-.5f) + .5f, grav.z * grav.z, 0);
}

#define FF_BIT_MASK(x) ((1 << x) - 1)
#define FF_FLOW_PRECISION 8
#define FF_FLOW_MAX (FF_BIT_MASK(FF_FLOW_PRECISION))
#define FF_FLOW_MAX_INV (1.0 / FF_FLOW_MAX)

inline float4 PackFlowSeam(in float2 target, in float rotation, in float4 texelSize)
{
#if defined(FF_FLOWTEX_COMPRESSED)
	// encode seam rotation in 5 bits
	uint rotEncoded = ((uint)(rotation * (1 << 5))) & FF_BIT_MASK(5);

	// Using an RGBA32 RenderTexture each channel has 8bit precision,
	// each target uv component is stored using 13bits, using some bit-magic to spread it over multiple 8bit texture channels
	// due to the 14bit precision of the target uv, (8191, 8191) is the maximum possible position that can be stored
	uint2 uv = (uint2) (target);
	uint2 low = uv & FF_BIT_MASK(FF_FLOW_PRECISION);
	uint2 high = (uv >> FF_FLOW_PRECISION) & FF_BIT_MASK(5); // 5 high-bits of the target x/y coordinate
	uint packedHigh = (high.x << 3) | (high.y & FF_BIT_MASK(3)); // 5 bits of high.x, and 3 lower bits of high.y
	uint packedRot = (high.y >> 3) | (rotEncoded << 2) | (1 << 7); // 2 high bits of high.y, 5 bits of rot encoded, and 1 high bit for detecting seems
	
	return float4(low, packedHigh, packedRot) * FF_FLOW_MAX_INV;
#else
	return float4(target * texelSize.xy, rotation, 1);
#endif
}

inline bool IsSeam(in float4 flow)
{
	return flow.w > 0;
}

inline bool IsZero(in float4 flow)
{
	return flow.x == 0 && flow.y == 0 && flow.z == 0 && flow.w == 0;
}

inline float2 UnpackSeamUV(in float4 flow, in float4 texelSize)
{
#if defined(FF_FLOWTEX_COMPRESSED)
	uint4 raw = flow * FF_FLOW_MAX;
	uint2 target = raw.xy;
	target.x += (raw.z >> 3) << FF_FLOW_PRECISION;
	target.y += ((raw.z & FF_BIT_MASK(3)) | (raw.w & FF_BIT_MASK(2)) << 3) << FF_FLOW_PRECISION;
	return target * texelSize.xy;
#else
	return flow.xy;
#endif
}

inline float2x2 UnpackSeamRotation(in float4 flow)
{
#if defined(FF_FLOWTEX_COMPRESSED)
	// unpack rotation to the 0-2*PI range
	float r = (((uint)(flow.w * FF_FLOW_MAX) >> 2) & FF_BIT_MASK(5)) * (PI2 / (1 << 5));
	return AngleToRotation(r);
#else
	return AngleToRotation(flow.z * PI2);
#endif
}

inline float2 UnpackGravity(in float4 flow) 
{
	return flow.xy * 2 - 1;
}

inline float UnpackRetainedFluid(in float4 flow)
{
	return flow.z;
}


// blit vertex shader base

struct blit_data
{
	float4 vertex : POSITION;
	float2 uv : TEXCOORD0;
};

struct blit_v2f
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
};

blit_v2f blitvert(blit_data v)
{
	blit_v2f o;
	o.vertex = UnityObjectToClipPos(v.vertex);
	o.uv = v.uv;
	return o;
}

#endif