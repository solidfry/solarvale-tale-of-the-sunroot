using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

public class UniStormAtmosphericFogPass : ScriptableRenderPass
{
    public UniStormAtmosphericFogFeature.Settings settings;

    RenderTextureDescriptor cameraRenderTextureDescriptor;

    RenderTargetIdentifier realSource;
    RenderTargetIdentifier source;
    RenderTargetHandle destination;

    string m_ProfilerTag;
    Material fogMaterial;


    public UniStormAtmosphericFogPass(string tag)
    {
        m_ProfilerTag = tag;
        Shader unistormAtmoshpericFogShader = Shader.Find("UniStorm/URP/UniStormAtmosphericFog");
#if UNITY_EDITOR
        if (unistormAtmoshpericFogShader == null) return;
#endif
        fogMaterial = new Material(unistormAtmoshpericFogShader);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cameraTextureDescriptor.depthBufferBits = 0;

        base.Configure(cmd, cameraTextureDescriptor);

        renderPassEvent = settings.renderPassEvent;
        //fogMaterial = settings.fogMaterial; 
        //if(fogMaterial == null) fogMaterial = new Material(settings.fogShader);
        cmd.GetTemporaryRT(destination.id, cameraTextureDescriptor);
        cameraRenderTextureDescriptor = cameraTextureDescriptor;
        SetupDirectionalLights();
    }

    internal void Setup(RenderTargetIdentifier cameraColorTarget)
    {
        realSource = cameraColorTarget;
    }

    private void SetupDirectionalLights()
    {
        Light[] directionalLights = Light.GetLights(LightType.Directional, 0);

        foreach(Light directionalLight in directionalLights)
        {
            if (directionalLight.gameObject.name.IndexOf("sun", StringComparison.OrdinalIgnoreCase) > 0)
                settings.SunSource = directionalLight;
            if (directionalLight.gameObject.name.IndexOf("moon", StringComparison.OrdinalIgnoreCase) > 0)
                settings.MoonSource = directionalLight;
        }
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
#if UNITY_EDITOR
        if (fogMaterial == null) return;
#endif
        //if (CheckResources() == false || (!distanceFog && !heightFog))
        //{
        //    Graphics.Blit(source, destination);
        //    return;
        //}

        CameraData cameraData = renderingData.cameraData;
        Camera camera = cameraData.camera;

        //Camera cam = GetComponent<Camera>();
        Transform camtr = camera.transform;
        //float camNear = camera.nearClipPlane;
        //float camFar = camera.farClipPlane;
        //float camFov = camera.fieldOfView;
        //float camAspect = camera.aspect;

        //Matrix4x4 frustumCorners = Matrix4x4.identity;

        //float fovWHalf = camFov * 0.5f;

        //Vector3 toRight = camtr.right * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
        //Vector3 toTop = camtr.up * camNear * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        //Vector3 topLeft = (camtr.forward * camNear - toRight + toTop);
        //float camScale = topLeft.magnitude * camFar / camNear;

        //topLeft.Normalize();
        //topLeft *= camScale;

        //Vector3 topRight = (camtr.forward * camNear + toRight + toTop);
        //topRight.Normalize();
        //topRight *= camScale;

        //Vector3 bottomRight = (camtr.forward * camNear + toRight - toTop);
        //bottomRight.Normalize();
        //bottomRight *= camScale;

        //Vector3 bottomLeft = (camtr.forward * camNear - toRight - toTop);
        //bottomLeft.Normalize();
        //bottomLeft *= camScale;

        //frustumCorners.SetRow(0, topLeft);
        //frustumCorners.SetRow(1, topRight);
        //frustumCorners.SetRow(2, bottomRight);
        //frustumCorners.SetRow(3, bottomLeft);

        var camPos = camtr.position;
        float FdotC = camPos.y - settings.height;
        float paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
        ///////
        Camera.MonoOrStereoscopicEye leftStereoscopicEye = Camera.MonoOrStereoscopicEye.Mono;
        if (XRSettings.enabled && XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced) 
            leftStereoscopicEye = Camera.MonoOrStereoscopicEye.Left;

        Vector3[] leftEyeFrustumCorners = new Vector3[4];
        Vector3[] rightEyeFrustumCorners = new Vector3[4];
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, leftStereoscopicEye, leftEyeFrustumCorners); //Camera.MonoOrStereoscopicEye.Left
        camera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), camera.farClipPlane, Camera.MonoOrStereoscopicEye.Right, rightEyeFrustumCorners);
        Matrix4x4 leftEyeFrustumMatrix = Matrix4x4.identity;
        Matrix4x4 rightEyeFrustumMatrix = Matrix4x4.identity;

        for (int i = 0; i < leftEyeFrustumCorners.Length; i++)
        {
            leftEyeFrustumCorners[i] = camera.transform.TransformVector(leftEyeFrustumCorners[i]);
        }

        for (int i = 0; i < rightEyeFrustumCorners.Length; i++)
        {
            rightEyeFrustumCorners[i] = camera.transform.TransformVector(rightEyeFrustumCorners[i]);
        }

        leftEyeFrustumMatrix.SetRow(0, leftEyeFrustumCorners[1]);
        leftEyeFrustumMatrix.SetRow(1, leftEyeFrustumCorners[2]);
        leftEyeFrustumMatrix.SetRow(2, leftEyeFrustumCorners[3]);
        leftEyeFrustumMatrix.SetRow(3, leftEyeFrustumCorners[0]);

        rightEyeFrustumMatrix.SetRow(0, rightEyeFrustumCorners[1]);
        rightEyeFrustumMatrix.SetRow(1, rightEyeFrustumCorners[2]);
        rightEyeFrustumMatrix.SetRow(2, rightEyeFrustumCorners[3]);
        rightEyeFrustumMatrix.SetRow(3, rightEyeFrustumCorners[0]);

        fogMaterial.SetMatrixArray("_FrustumCornersWSArray", new Matrix4x4[2] { leftEyeFrustumMatrix, rightEyeFrustumMatrix});
        //////
        //fogMaterial.SetMatrix("_FrustumCornersWS", frustumCorners);
        fogMaterial.SetVector("_CameraWS", camPos);
        fogMaterial.SetVector("_HeightParams", new Vector4(settings.height, FdotC, paramK, settings.heightDensity * 0.5f));
        fogMaterial.SetVector("_DistanceParams", new Vector4(-Mathf.Max(settings.startDistance, 0.0f), 0, 0, 0));

        if(settings.SunSource != null)
            fogMaterial.SetVector("_SunVector", settings.SunSource.transform.rotation * -Vector3.forward);
        if(settings.MoonSource != null)
            fogMaterial.SetVector("_MoonVector", settings.MoonSource.transform.rotation * -Vector3.forward);
        fogMaterial.SetFloat("_SunIntensity", settings.SunIntensity);
        fogMaterial.SetFloat("_MoonIntensity", settings.MoonIntensity);
        fogMaterial.SetFloat("_SunAlpha", settings.SunFalloffIntensity);
        fogMaterial.SetColor("_SunColor", settings.SunColor);
        fogMaterial.SetColor("_MoonColor", settings.MoonColor);

        fogMaterial.SetColor("_UpperColor", settings.TopColor);
        fogMaterial.SetColor("_BottomColor", settings.BottomColor);
        fogMaterial.SetFloat("_FogBlendHeight", settings.BlendHeight);
        fogMaterial.SetFloat("_FogGradientHeight", settings.FogGradientHeight);

        fogMaterial.SetFloat("_SunControl", settings.SunControl);
        fogMaterial.SetFloat("_MoonControl", settings.MoonControl);

        if (settings.Dither == UniStormAtmosphericFogFeature.DitheringControl.Enabled)
        {
            fogMaterial.SetFloat("_EnableDithering", 1);
            fogMaterial.SetTexture("_NoiseTex", settings.NoiseTexture);
        }
        else
        {
            fogMaterial.SetFloat("_EnableDithering", 0);
        }

        var sceneMode = RenderSettings.fogMode;
        var sceneDensity = RenderSettings.fogDensity;
        var sceneStart = RenderSettings.fogStartDistance;
        var sceneEnd = RenderSettings.fogEndDistance;
        Vector4 sceneParams;
        bool linear = (sceneMode == FogMode.Linear);
        float diff = linear ? sceneEnd - sceneStart : 0.0f;
        float invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
        sceneParams.x = sceneDensity * 1.2011224087f;
        sceneParams.y = sceneDensity * 1.4426950408f;
        sceneParams.z = linear ? -invDiff : 0.0f;
        sceneParams.w = linear ? sceneEnd * invDiff : 0.0f;
        fogMaterial.SetVector("_SceneFogParams", sceneParams);
        fogMaterial.SetVector("_SceneFogMode", new Vector4((int)sceneMode, settings.useRadialDistance ? 1 : 0, 0, 0));

        int pass = 0;
        if (settings.distanceFog && settings.heightFog)
            pass = 0; // distance + height
        else if (settings.distanceFog)
            pass = 1; // distance only
        else
            pass = 2; // height only

        //CustomGraphicsBlit(source, destination, fogMaterial, pass);
        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        source = realSource;
        cmd.Clear();

        if (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced
            && cameraRenderTextureDescriptor.volumeDepth == 2)
        {
            cmd.EnableShaderKeyword("STEREO_INSTANCING_ON");
        }

        if (XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.MultiPass
            && camera.stereoActiveEye != Camera.MonoOrStereoscopicEye.Mono && camera.stereoEnabled == true)
        {
            cmd.EnableShaderKeyword("STEREO_MULTIVIEW_ON");
        }

        cmd.Blit(source, destination.id, fogMaterial, pass);
        cmd.Blit(destination.id, source);

        cmd.DisableShaderKeyword("STEREO_INSTANCING_ON");
        cmd.DisableShaderKeyword("STEREO_MULTIVIEW_ON");

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    private Camera.MonoOrStereoscopicEye GetStereoscopicLeftEye(Camera camera)
    {

        Camera.MonoOrStereoscopicEye leftEye;
        if (camera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left
            && XRSettings.stereoRenderingMode == XRSettings.StereoRenderingMode.SinglePassInstanced)
        {
            leftEye = Camera.MonoOrStereoscopicEye.Left;
        }
        else
        {
            leftEye = Camera.MonoOrStereoscopicEye.Mono;
        }
        return leftEye;
    }
}
