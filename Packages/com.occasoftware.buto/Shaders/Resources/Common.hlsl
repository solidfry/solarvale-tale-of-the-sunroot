#ifndef OCCASOFTWARE_BUTO_COMMON_INCLUDED
#define OCCASOFTWARE_BUTO_COMMON_INCLUDED
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

SamplerState buto_linear_repeat_sampler;
SamplerState buto_linear_clamp_sampler;
SamplerState buto_point_clamp_sampler;

// Shared variables
float3 fog_volume_size;
float3 fog_cell_size; // (1.0f / fog_volume_size);
float _IntegrationRate;



/////////////////////////////////////
//  Basic math functions           //
/////////////////////////////////////


// Returns % between start and stop
float InverseLerp(float start, float stop, float value)
{
  float x = (value - start) / (stop - start);
  return saturate(x);
}

float buto_Remap(float inStart, float inStop, float outStart, float outStop, float v)
{
	float t = InverseLerp(inStart, inStop, v); 
	return lerp(outStart, outStop, saturate(t));
}


float InverseLerp0N(float stop, float value)
{
  return clamp(value / stop, 0, stop);
}


float Remap0N(float inStop, float outStart, float outStop, float v)
{
  float t = v / inStop;
	return lerp(outStart, outStop, saturate(t));
}


bool IsOrthographicPerspective()
{
	return unity_OrthoParams.w == 1.0;
}

float CheckDepthDirection(float depthRaw)
{
    #if UNITY_REVERSED_Z
        depthRaw = 1.0 - depthRaw;
    #endif
    return depthRaw;
}

float GetDepthEye(float depthRaw)
{
    if(IsOrthographicPerspective())
    {
        depthRaw = CheckDepthDirection(depthRaw);
        return lerp(_ProjectionParams.y, _ProjectionParams.z, depthRaw);
    }
                
    return LinearEyeDepth(depthRaw, _ZBufferParams);
}

float GetDepth01(float depthRaw)
{
    if(IsOrthographicPerspective())
    {
        return CheckDepthDirection(depthRaw);
    }
                
    return Linear01Depth(depthRaw, _ZBufferParams);
}



////////////////////////////////////
// Transformations                //
////////////////////////////////////

float _MaxDistanceVolumetric;
float _depth_ratio;
float _InverseMaxDistanceVolumetric; // 1.0f / MaxDistanceVolumetric
float _inverse_depth_ratio; // 1.0f / depth ratio

float _InverseLightDistanceVolumetric; // 1.0f / MaxLightDistance;

float3 os_WorldSpaceCameraPosition;
float3 GetCameraPosition()
{
	return os_WorldSpaceCameraPosition.xyz;
}




float2 fogNearSize;
float2 fogFarSize;
float2 fogNearZ;
float2 fogFarZ;

//#define _USE_EXPERIMENTAL_SAMPLING
#define _PERCENTILE 0.8
#define _RANGE 0.05

float RescaleDepth(float x, float percentile, float range)
{
  #ifdef _USE_EXPERIMENTAL_SAMPLING
  if(x < percentile){
    x = Remap(0, percentile, 0, 1, x);
    x = pow(x, _depth_ratio);
    x *= range;
  }
  else{
    x = Remap(percentile, 1.0, 0, 1.0, x);
    x = pow(x, _depth_ratio);
    x = (x * (1 - range)) + range;
  }
  
  x = saturate(x);
  return x;
  #else
  return pow(abs(x), _depth_ratio);
  #endif
}

float InverseRescaleDepth(float y, float percentile, float range)
{
	#ifdef _USE_EXPERIMENTAL_SAMPLING
	 if (y < range) {
        y /= range;
		y = pow(y, _inverse_depth_ratio);
        y = Remap(0, 1, 0, percentile, y);
    } else {
        y = (y - range) / (1.0 - range);
		y = pow(abs(y), _inverse_depth_ratio);
        y = Remap(0, 1, percentile, 1.0, y);
    }

    return saturate(y); // Ensure the result is in the [0, 1] range
	#else
	return pow(abs(y), _inverse_depth_ratio);
	#endif
}

float3 UvwToViewPosition(float3 uvw)
{
	float3 viewPosition = uvw;
	viewPosition.xy = (viewPosition.xy * 2.0) - 1.0;
	viewPosition.z = RescaleDepth(viewPosition.z, _PERCENTILE, _RANGE);
	viewPosition.xy *= lerp(fogNearSize, fogFarSize, viewPosition.z);
	viewPosition.z *= _MaxDistanceVolumetric;
	return viewPosition;
}

float3 ViewPositionToUvw(float3 viewPosition)
{
	float3 uvw = viewPosition;
	uvw.z *= _InverseMaxDistanceVolumetric;
  uvw.xy = uvw.xy / (lerp(fogNearSize, fogFarSize, uvw.z) + 1e-7);
	uvw.z = InverseRescaleDepth(uvw.z, _PERCENTILE, _RANGE);
	uvw.xy = (uvw.xy * 0.5) + 0.5;
	return saturate(uvw);
}


// Computing the ray direction in CS requires the w component of the UV to be 1 (!) rather than -1. idk why :) beep boop
float4x4 os_CameraInvProjection;
float4x4 os_CameraToWorld;
float4x4 os_WorldToCamera;
float3 os_CameraForward;
float3 GetRayDirectionCompute(float2 uv)
{
  if (IsOrthographicPerspective())
  {
    return os_CameraForward;
  }
  float3 viewVector = mul(os_CameraInvProjection, float4((uv * 2.0) - 1.0, 0.0, 1.0)).xyz;
	viewVector = mul(os_CameraToWorld, float4(viewVector, 0.0)).xyz;
	float3 rayDirection = viewVector;

	rayDirection = viewVector / length(viewVector);
	return rayDirection;
}

float GetRayLengthCompute(float2 uv)
{
  if (IsOrthographicPerspective())
  {
    return 1.0;
  }
	float3 viewVector = mul(os_CameraInvProjection, float4(uv * 2.0 - 1.0, 0.0, 1.0)).xyz;
	viewVector = mul(os_CameraToWorld, float4(viewVector, 0.0)).xyz;
	return length(viewVector);
}

float cameraFarPlane;
float3 UvwToWorldPosition(float3 uvw)
{
  float3 positionVS = UvwToViewPosition(uvw);
  float rayLength = GetRayLengthCompute(uvw.xy);

  positionVS.z = -positionVS.z;
  positionVS.xyz = positionVS.xyz / (rayLength + 1e-7);
  float3 positionWS = mul(os_CameraToWorld, float4(positionVS.xyz, 1.0)).xyz;
  return positionWS;
}


float3 GetRayOrigin(float3 uvw)
{
  return UvwToWorldPosition(float3(uvw.x, uvw.y, 0));
}

float3 DirectionAndDistanceToWorldPosition(float3 uvw, float3 rayDirection, float depth01)
{
    return GetRayOrigin(uvw) + rayDirection * RescaleDepth(depth01, _PERCENTILE, _RANGE) * _MaxDistanceVolumetric;
}

float3 WorldPositionToUvw(float3 positionWS)
{
    float4 positionVS = mul(os_WorldToCamera, float4(positionWS.xyz, 1.0));
    positionVS.xyz /= positionVS.w;
    positionVS.z = -positionVS.z;
    return ViewPositionToUvw(positionVS.xyz);
}

float4x4 toPreviousView;

struct ReprojectionData
{
	float3 uvwPrevious;
	bool isValid;
};

// I referenced Godot's implementation of reprojection a froxel volume for this specific UVW to View transformation
// https://github.com/godotengine/godot/blob/60f3b7967cbd00b4e1f52d33d372646f7bec02f6/servers/rendering/renderer_rd/shaders/environment/volumetric_fog.glsl#L46

ReprojectionData CalculateReprojection(float3 uvw)
{
	ReprojectionData reprojectionData;
	
	float3 viewPosition = UvwToViewPosition(uvw);

	float3 previousView = mul(toPreviousView, float4(viewPosition.xyz, 1.0)).xyz;
	
	reprojectionData.uvwPrevious = ViewPositionToUvw(previousView);
	if(all(reprojectionData.uvwPrevious > 0.0) && all(reprojectionData.uvwPrevious < 1.0))
	{
		reprojectionData.isValid = true;
	}
	else
	{
		reprojectionData.isValid = false;
	}
	return reprojectionData;
}

float3 IdToUVW(uint3 id, float3 fogCellSize)
{
	return saturate((id * fogCellSize) + (fogCellSize * 0.5));
}


/////////////////////////////////////
//  SDFs                           //
/////////////////////////////////////

struct Box
{
	float3 position;
	float3 size;
};

struct Sphere
{
	float3 position;
	float radius;
};


float SdBox(float3 rayPosition, Box box)
{
	float3 v = abs(rayPosition - box.position) - box.size * 0.5;
	return length(max(v, 0)) + min(max(v.x, max(v.y,v.z)), 0);
}

float SdSphere(float3 rayPosition, Sphere sphere)
{
	return max(length(rayPosition - sphere.position) - sphere.radius, 0);
}


/////////////////////////////////////
//  Halton Sampler                 //
/////////////////////////////////////

float rand(float3 vec)
{
	float a = 12.9898;
	float b = 78.233;
	float c = 153.332;
	float d = 43758.5453;
	float dt= dot(vec, float3(a,b,c));
	float sn= dt % 3.14;
	return frac(sin(sn) * d);
}

static const int HALTON_ARRAY_SIZE = 16;
int GetHaltonId(uint3 id)
{
	return (rand(id) * HALTON_ARRAY_SIZE + (_Time.y * 60.0)) % HALTON_ARRAY_SIZE;
}

static const float3 halton[HALTON_ARRAY_SIZE] = {
    float3(0.5,0.3333333,0.2),
    float3(0.25,0.6666667,0.4),
    float3(0.75,0.1111111,0.6),
    float3(0.125,0.4444444,0.8),
    float3(0.625,0.7777778,0.04),
    float3(0.375,0.2222222,0.24),
    float3(0.875,0.5555556,0.44),
    float3(0.0625,0.8888889,0.64),
    float3(0.5625,0.03703704,0.84),
    float3(0.3125,0.3703704,0.08),
    float3(0.8125,0.7037037,0.28),
    float3(0.1875,0.1481481,0.48),
    float3(0.6875,0.4814815,0.68),
    float3(0.4375,0.8148148,0.88),
    float3(0.9375,0.2592593,0.12),
    float3(0.03125,0.5925926,0.32),
};

#endif