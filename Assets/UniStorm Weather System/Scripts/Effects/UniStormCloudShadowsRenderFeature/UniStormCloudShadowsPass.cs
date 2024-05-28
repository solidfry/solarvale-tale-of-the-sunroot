using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UniStormCloudShadowsPass : ScriptableRenderPass
{
    public UniStormCloudShadowsFeature.Settings settings;

    RenderTargetIdentifier realSource;
    RenderTargetIdentifier source;
    RenderTargetHandle destination;

    Material screenSpaceShadowsMaterial;

    string m_ProfilerTag;

    public UniStormCloudShadowsPass(string tag)
    {
        m_ProfilerTag = tag;
        Shader unistormCloudShadowsShader = Shader.Find("UniStorm/URP/UniStormCloudShadows");
#if UNITY_EDITOR
        if (unistormCloudShadowsShader == null) return;
#endif
        screenSpaceShadowsMaterial = new Material(unistormCloudShadowsShader);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        base.Configure(cmd, cameraTextureDescriptor);

        renderPassEvent = settings.renderPassEvent;
        cmd.GetTemporaryRT(destination.id, cameraTextureDescriptor);
    }

    public void Setup(RenderTargetIdentifier cameraColorTargetIdent)
    {
        realSource = cameraColorTargetIdent;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
#if UNITY_EDITOR
        if (screenSpaceShadowsMaterial == null) return;
#endif
        CameraData cameraData = renderingData.cameraData;
        Camera camera = cameraData.camera;

        CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
        source = realSource;
        cmd.Clear();

        //Set shader properties
        screenSpaceShadowsMaterial.SetMatrix("_CamToWorld", camera.cameraToWorldMatrix); // UniStormSystem.Instance.PlayerCamera.cameraToWorldMatrix);
        screenSpaceShadowsMaterial.SetTexture("_CloudTex", settings.CloudShadowTexture);
        screenSpaceShadowsMaterial.SetFloat("_CloudTexScale", settings.CloudTextureScale + (settings.m_CurrentCloudHeight * 0.000001f) * 2); //UniStormSystem.Instance.m_CurrentCloudHeight
        screenSpaceShadowsMaterial.SetFloat("_BottomThreshold", settings.BottomThreshold);
        screenSpaceShadowsMaterial.SetFloat("_TopThreshold", settings.TopThreshold);
        screenSpaceShadowsMaterial.SetFloat("_CloudShadowIntensity", settings.ShadowIntensity);
        screenSpaceShadowsMaterial.SetFloat("_CloudMovementSpeed", settings.CloudSpeed * -0.005f); //UniStormSystem.Instance.CloudSpeed
        screenSpaceShadowsMaterial.SetVector("_SunDirection", new Vector3(settings.ShadowDirection.x, settings.ShadowDirection.y, settings.ShadowDirection.z));
        screenSpaceShadowsMaterial.SetFloat("_Fade", settings.Fade);
        screenSpaceShadowsMaterial.SetFloat("_normalY", settings.NormalY);

        //Execute the shader on input texture (src) and write to output (dest)
        cmd.Blit(source, destination.id, screenSpaceShadowsMaterial);
        cmd.Blit(destination.id, source);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }
}
