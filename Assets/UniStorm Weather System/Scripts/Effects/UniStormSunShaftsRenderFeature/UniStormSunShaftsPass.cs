using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

public class UniStormSunShaftsPass : ScriptableRenderPass
{
    public FilterMode filterMode { get; set; }
    public UniStormSunShaftsFeature.Settings settings;

    RenderTextureDescriptor cameraRenderTextureDescriptor;

    RenderTargetIdentifier realSource;
    RenderTargetIdentifier source;
    RenderTargetHandle destination;

    Material sunShaftsMaterial = null;

    string m_ProfilerTag;

    private Transform sunTransform;

    public Transform SunTransform
    {
        get
        {
            if (sunTransform == null)
            {
                //Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
                Light[] lights = Light.GetLights(LightType.Directional, ~0);
                if (lights.Length > 0)
                {
                    Light sunLight = lights.FirstOrDefault(x => x.name.Equals(settings.celestialName));
                    if (sunLight != null)
                    {
                        sunTransform = sunLight.transform.GetChild(0);
                    }
                }
            }
            
            return sunTransform;
        }
    }

    public UniStormSunShaftsPass(string tag)
    {
        m_ProfilerTag = tag;
        Shader unistormSunShaftsShader = Shader.Find("UniStorm/URP/UniStormSunShafts");
#if UNITY_EDITOR
        if (unistormSunShaftsShader == null) return;
#endif
        sunShaftsMaterial = new Material(unistormSunShaftsShader);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        base.Configure(cmd, cameraTextureDescriptor);

        renderPassEvent = settings.renderPassEvent;

        cmd.GetTemporaryRT(destination.id, cameraTextureDescriptor);
        cameraRenderTextureDescriptor = cameraTextureDescriptor;
    }

    public void Setup(RenderTargetIdentifier cameraColorTargetIdent)
    {
        realSource = cameraColorTargetIdent;
    }

    /// <inheritdoc/>
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
#if UNITY_EDITOR
        if (sunShaftsMaterial == null) return;
#endif
        CameraData cameraData = renderingData.cameraData;
        Camera camera = cameraData.camera;

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

        int divider = 4;
        if (settings.resolution == UniStormSunShaftsFeature.SunShaftsResolution.Normal)
            divider = 2;
        else if (settings.resolution == UniStormSunShaftsFeature.SunShaftsResolution.High)
            divider = 1;

        Vector3 vl = Vector3.one * 0.5f;
        Vector3 vr = Vector3.one * 0.5f;

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

        if (SunTransform)
        {
            var sunPosition = SunTransform.position;
            vl = camera.WorldToViewportPoint(sunPosition, leftEye);
            vr = camera.WorldToViewportPoint(sunPosition, Camera.MonoOrStereoscopicEye.Right);
        }
        else
        {
            vl = new Vector3(0.5f, 0.5f, 0.0f);
            vr = new Vector3(0.5f, 0.5f, 0.0f);
        }

        int width = cameraRenderTextureDescriptor.width;
        int height = cameraRenderTextureDescriptor.height;

        int rtW = width / divider; //source.width / divider;
        int rtH = height / divider; //source.height / divider;

        RenderTexture lrColorB;

        RenderTexture lrDepthBuffer = GetTemporaryBlurStageRenderTexture(cameraRenderTextureDescriptor, rtW, rtH, 0);

        sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(1.0f, 1.0f, 0.0f, 0.0f) * settings.sunShaftBlurRadius);
        sunShaftsMaterial.SetVectorArray("_SunPositionArray", new Vector4[2] { new Vector4(vl.x, vl.y, vl.z, settings.maxRadius), new Vector4(vr.x, vr.y, vr.z, settings.maxRadius) });
        sunShaftsMaterial.SetVector("_SunThreshold", settings.sunThreshold);

        if (!settings.useDepthTexture)
        {
#if UNITY_5_6_OR_NEWER
            var format = camera.allowHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
#else
                var format = camera.hdr ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
#endif
            RenderTexture tmpBuffer = RenderTexture.GetTemporary(width, height, 0, format);

            RenderTargetIdentifier tmpBufferRTId = new RenderTargetIdentifier(tmpBuffer);

            ConfigureTarget(tmpBufferRTId);

            sunShaftsMaterial.SetTexture("_Skybox", tmpBuffer);
            cmd.Blit(source, lrDepthBuffer, sunShaftsMaterial, 3);

            RenderTexture.ReleaseTemporary(tmpBuffer);
        }
        else
        {
            cmd.Blit(source, lrDepthBuffer, sunShaftsMaterial, 2);
        }

        // paint a small black small border to get rid of clamping problems
        //DrawBorder(cmd, lrDepthBuffer, simpleClearMaterial); ////////////////////////////////////////// FIX

        // radial blur:

        settings.radialBlurIterations = Mathf.Clamp(settings.radialBlurIterations, 1, 4);

        float ofs = settings.sunShaftBlurRadius * (1.0f / 768.0f);

        sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
        //sunShaftsMaterial.SetVector("_SunPosition", new Vector4(vl.x, vl.y, vl.z, settings.maxRadius));
        sunShaftsMaterial.SetVectorArray("_SunPositionArray", new Vector4[2] { new Vector4(vl.x, vl.y, vl.z, settings.maxRadius), new Vector4(vr.x, vr.y, vr.z, settings.maxRadius) });

        for (int it2 = 0; it2 < settings.radialBlurIterations; it2++)
        {
            // each iteration takes 2 * 6 samples
            // we update _BlurRadius each time to cheaply get a very smooth look

            lrColorB = GetTemporaryBlurStageRenderTexture(cameraRenderTextureDescriptor, rtW, rtH, 0);

            cmd.Blit(lrDepthBuffer, lrColorB, sunShaftsMaterial, 1);
            RenderTexture.ReleaseTemporary(lrDepthBuffer);

            ofs = settings.sunShaftBlurRadius * (((it2 * 2.0f + 1.0f) * 6.0f)) / 768.0f;
            sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));


            lrDepthBuffer = GetTemporaryBlurStageRenderTexture(cameraRenderTextureDescriptor, rtW, rtH, 0);
            cmd.Blit(lrColorB, lrDepthBuffer, sunShaftsMaterial, 1);
            RenderTexture.ReleaseTemporary(lrColorB);

            ofs = settings.sunShaftBlurRadius * (((it2 * 2.0f + 2.0f) * 6.0f)) / 768.0f;
            sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(ofs, ofs, 0.0f, 0.0f));
        }

        // put together:

        if (vl.z >= 0.0f)
            sunShaftsMaterial.SetVector("_SunColor", new Vector4(settings.sunColor.r, settings.sunColor.g, settings.sunColor.b, settings.sunColor.a) * settings.sunShaftIntensity);
        else
            sunShaftsMaterial.SetVector("_SunColor", Vector4.zero); // no backprojection !
        sunShaftsMaterial.SetTexture("_ColorBuffer", lrDepthBuffer);
        cmd.Blit(source, destination.id, sunShaftsMaterial, (settings.screenBlendMode == UniStormSunShaftsFeature.ShaftsScreenBlendMode.Screen) ? 0 : 4);
        cmd.Blit(destination.id, source);

        cmd.DisableShaderKeyword("STEREO_INSTANCING_ON");
        cmd.DisableShaderKeyword("STEREO_MULTIVIEW_ON");

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        RenderTexture.ReleaseTemporary(lrDepthBuffer);

        CommandBufferPool.Release(cmd);
    }

    private RenderTexture GetTemporaryBlurStageRenderTexture(RenderTextureDescriptor renderTextureDescriptor, int width, int height, int depthBuffer)
    {
        RenderTextureDescriptor rtd = renderTextureDescriptor;
        rtd.width = width;
        rtd.height = height;
        rtd.depthBufferBits = depthBuffer;
        rtd.msaaSamples = 1;

        return RenderTexture.GetTemporary(rtd);
    }
}