#ifndef __TERRAIN_INC
#define __TERRAIN_INC

float InternalUnpackHeightmap(float4 v)
{
    return v.r;
}

float InternalPackHeightmap(float v)
{
    return float4(v, v, v, v);
}

#endif