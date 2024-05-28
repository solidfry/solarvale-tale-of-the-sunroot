#ifndef PW_DETAILINDIRECT_URP_INCLUDED
#define PW_DETAILINDIRECT_URP_INCLUDED

void Translucency_float(float3 worldPos, half3 normal,half3 viewDir,half thickness,half3 baseIn, out half3 baseO, out half3 transO)
{
    float inverseThickness = 1-thickness;
    inverseThickness *= inverseThickness * inverseThickness;
    #ifdef SHADERGRAPH_PREVIEW
    transO = 1;
    baseO = 1;
    #else
    half4 shadowCoord = TransformWorldToShadowCoord(worldPos);
    Light light = GetMainLight(shadowCoord);
    half3 lightDir = (half3)light.direction;
    half3 lightCol = light.color;
    half3 HalfVec = lightDir + (normal * thickness);
    half viewDotHalfVec = dot(normalize(viewDir), -HalfVec);
    viewDotHalfVec = viewDotHalfVec * 0.5f + 0.5f;
    half  backlight = pow(viewDotHalfVec,6 * inverseThickness + 1 ) * inverseThickness;
    baseO = baseIn * (1- inverseThickness);
    transO = backlight.xxx * lightCol.rgb * max(light.shadowAttenuation,min(inverseThickness,0.5));
    #endif
}

void TransMainLight_float(float3 i_worldPos, half3 i_normal,half3 i_viewDir,half i_thickness,half i_inverseThickness, out half3 o_mainLightTrans)
{
    o_mainLightTrans = 0;
    #ifndef SHADERGRAPH_PREVIEW
        half4 shadowCoord = TransformWorldToShadowCoord(i_worldPos);
        Light light = GetMainLight(shadowCoord);
        i_viewDir = SafeNormalize(i_viewDir);
        half3 lightDir = (half3)light.direction;
        half3 lightCol = light.color;
        half3 HalfVec = normalize(lightDir + (i_normal * i_thickness));
        half viewDotHalfVec = dot(i_viewDir, -HalfVec);
        viewDotHalfVec = viewDotHalfVec * 0.5f + 0.5f;
        half  backlight = pow(viewDotHalfVec,6 * i_inverseThickness + 1 ) * i_inverseThickness;
        o_mainLightTrans = backlight.xxx * lightCol.rgb * light.shadowAttenuation;
    #endif
}

void TransAdditionalLights_float(float3 i_worldPos, half3 i_normal,half3 i_viewDir,half i_thickness,half i_inverseThickness, out half3 o_additionalLightsTrans)
{
    o_additionalLightsTrans = 0;
    #ifndef  SHADERGRAPH_PREVIEW
        i_worldPos = normalize(i_worldPos);
        i_viewDir = SafeNormalize(i_viewDir);   
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, i_worldPos);
        half3 lightDir = (half3)light.direction;
        half3 lightCol = light.color;
        half3 HalfVec = lightDir + (i_normal * i_thickness);
        half  viewDotHalfVec = pow(saturate(dot(i_viewDir, -HalfVec)),8 * i_inverseThickness + 1 ) * i_inverseThickness;
        o_additionalLightsTrans += viewDotHalfVec.xxx * lightCol.rgb * max(light.shadowAttenuation,min(i_inverseThickness,0.5)) * light.distanceAttenuation;
    }
    #endif
}

void TransData_float(half i_thickness, half3 i_baseColor, out half o_inverseThickness, out half3 o_baseColor)
{
    float inverseThickness = 1-i_thickness;
    o_inverseThickness = inverseThickness * inverseThickness * inverseThickness;
    o_baseColor = i_baseColor * (1 - i_thickness * 0.33);
}

#endif
