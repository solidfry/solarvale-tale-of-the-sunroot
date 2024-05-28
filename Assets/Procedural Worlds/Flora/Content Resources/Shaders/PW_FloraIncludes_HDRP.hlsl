#ifndef PW_DETAILINDIRECT_INCLUDED
#define PW_DETAILINDIRECT_INCLUDED

#ifndef LOD_FADE_CROSSFADE
sampler2D unity_DitherMask;
#endif

struct DetailData {
    float4x4 obj2World;
    float4x4 world2Obj;
};

float _detailData;
float4 _detailBehaviorData;
float4 _PW_DetailGlobals;

float FastDistance2D(float3 v1, float3 v2)
{
    float dx = abs(v1.x - v2.x);
    float dz = abs(v1.z - v2.z);
    return 0.394 * (dx + dz) + 0.554 * max(dx, dz);
}

void ColorVariationMix_float(in float4 colorA, in float4 colorB, in float3 texColor, out float3 colorOut)
{
    float greyscaled = dot(texColor.rgb, half3(0.333, 0.333, 0.333));
    float4 tintCol = lerp(colorA, colorB, _detailData.x);
    texColor.rgb = lerp(texColor.rgb, greyscaled.xxx, tintCol.a);
    colorOut = texColor.rgb * tintCol.rgb;
}

void DistanceFade_float(in float dist, out float distFadeOut)
{
  //_detailBehaviorData *= _PW_DetailGlobals.w;
  _detailBehaviorData.x -= _detailBehaviorData.y;
  float2 fade = saturate(max(dist.xx - _detailBehaviorData.xz, float2(0.001f,0.001f)) / _detailBehaviorData.yw);
  distFadeOut = (1-fade.x) * fade.y;
}

void DitherCrossFade(float2 vpos,float fade)
{
    vpos /= 4; // the dither mask texture is 4x4
    float mask = tex2D(unity_DitherMask, vpos).a; 
    clip(fade * fade - 0.001 - mask * mask); // needs to be improved
}

void DitherCrossFade_float(in float4 i_screenPos, float i_fade, float i_distance,float i_cutoff, out float o_alpha)
{
    #if (SHADERPASS == SHADERPASS_SHADOWS) || (SHADERPASS == SHADERPASS_RAYTRACING_VISIBILITY)
        o_alpha = i_fade;
    #else
   
    float2 uv = i_screenPos.xy * _ScreenParams.xy;
    float DITHER_THRESHOLDS[16] =
    {
        1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
        13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
        4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
        16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
    };
    uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
    float dither = DITHER_THRESHOLDS[index];

    // way too branched must be a better way.
    i_fade = 1-i_fade;
    bool flipfade = i_distance > (_detailBehaviorData.x - _detailBehaviorData.y) ? true : false;
    i_fade = flipfade == true ? -i_fade : i_fade;
    i_fade = 1-i_fade;
   
    float fade = i_fade - dither;
    o_alpha = flipfade == true ? 1-fade : fade;
    o_alpha = saturate(o_alpha) * (1.0f - i_cutoff) + i_cutoff;
    
    #endif
    
}

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
#if defined(SHADER_API_GLCORE) || defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_METAL) || defined(SHADER_API_VULKAN) || defined(SHADER_API_PSSL) || defined(SHADER_API_XBOXONE)
    StructuredBuffer<DetailData> detailBuffer;
#endif	
#endif


void setup()
{
#undef unity_ObjectToWorld Use_Macro_UNITY_MATRIX_M_instead_of_unity_ObjectToWorld
#undef unity_WorldToObject Use_Macro_UNITY_MATRIX_I_M_instead_of_unity_WorldToObject
#define unity_ObjectToWorld unity_ObjectToWorld
#define unity_WorldToObject unity_WorldToObject
    #ifndef	_VRI_DEBUG
        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            unity_ObjectToWorld = detailBuffer[unity_InstanceID].obj2World;
            unity_WorldToObject = detailBuffer[unity_InstanceID].world2Obj;
    
            _detailData = unity_ObjectToWorld._41; 
            unity_ObjectToWorld._41 = 0;

            // Temporary solution to GPU culling until two stage compute buffers are implemented.
            // moves instance behind camera so polygons arnt rasterized when out side of valid near and far bounds.
            // second stage will remove vertex shader overheads as well.
    
            float3 positionVector  = _WorldSpaceCameraPos - unity_ObjectToWorld._m03_m13_m23;
            float distance = length(positionVector);
            float4 ranges = _detailBehaviorData;// *= _PW_DetailGlobals.w;
            if (distance > ranges.x || distance < ranges.z)
            {
                float3 cameraDirectionVector = mul(UNITY_MATRIX_M, transpose(mul(UNITY_MATRIX_I_M, UNITY_MATRIX_I_V)) [2].xyz) * 500;

                float3 behindCamoffset = _WorldSpaceCameraPos + cameraDirectionVector;
   
                // zero rotation and scale
                unity_ObjectToWorld._m00_m01_m02 = 0;
                unity_ObjectToWorld._m10_m11_m12 = 0;
                unity_ObjectToWorld._m20_m21_m22 = 0;
   
                // copy 
                unity_WorldToObject = unity_ObjectToWorld;
   
                // set positions
                unity_ObjectToWorld._m03_m13_m23 = behindCamoffset;
                unity_WorldToObject._m03_m13_m23 -= behindCamoffset;
            }
            
        #endif
    #endif
}

//Dummy Flora function to insert Indirect Instancing Pre Processors
void FloraIndirect_float(float3 inPos, out float3 outPos)
{
    outPos = inPos;
}

void FloraIndirect_half(float3 inPos, out float3 outPos)
{
    outPos = inPos;
}


#endif
