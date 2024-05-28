using UnityEngine;
using UnityEngine.Rendering.Universal;


public class UniStormSunShaftsFeature : ScriptableRendererFeature
{
    public enum BufferType
    {
        CameraColor,
        Custom
    }

    public enum SunShaftsResolution
    {
        Low = 0,
        Normal = 1,
        High = 2,
    }

    public enum ShaftsScreenBlendMode
    {
        Screen = 0,
        Add = 1,
    }

    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

        //public Shader sunShaftsShader;
        public SunShaftsResolution resolution = SunShaftsResolution.Normal;
        public ShaftsScreenBlendMode screenBlendMode = ShaftsScreenBlendMode.Screen;

        public string celestialName;
        public int radialBlurIterations = 2;
        public Color sunColor = Color.white;
        public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);
        public float sunShaftBlurRadius = 2.5f;
        public float sunShaftIntensity = 1.15f;
        public float maxRadius = 0.75f;
        public bool useDepthTexture = true;
    }

    public Settings settings = new Settings();

    UniStormSunShaftsPass sunShaftsPass;

    public override void Create()
    {
        sunShaftsPass = new UniStormSunShaftsPass(name);
    }

    #if UNITY_2022_1_OR_NEWER
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        sunShaftsPass.Setup(renderer.cameraColorTargetHandle);  // use of target after allocation
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.isEnabled)
            return;

        sunShaftsPass.settings = settings;
        renderer.EnqueuePass(sunShaftsPass);
    }
    #else
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.isEnabled)
            return;

        sunShaftsPass.settings = settings;
        sunShaftsPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(sunShaftsPass);
    }
    #endif
}