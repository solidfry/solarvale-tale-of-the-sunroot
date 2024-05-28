using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UniStormCloudShadowsFeature : ScriptableRendererFeature
{

    [System.Serializable]
    public class Settings
    {
        public bool isEnabled = true;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

        public float Fade = 0.33f;
        public RenderTexture CloudShadowTexture;
        public Color ShadowColor = Color.white;
        public float CloudTextureScale = 0.1f;
        public float BottomThreshold = 0f;
        public float TopThreshold = 1f;
        public float ShadowIntensity = 1f;
        public Material ScreenSpaceShadowsMaterial;
        public Vector3 ShadowDirection;

        public float m_CurrentCloudHeight;
        public int CloudSpeed;

        public float NormalY;
    }

    public Settings settings = new Settings();

    UniStormCloudShadowsPass cloudShadowsPass;

    public override void Create()
    {
        cloudShadowsPass = new UniStormCloudShadowsPass(name);
    }

    #if UNITY_2022_1_OR_NEWER
    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        cloudShadowsPass.Setup(renderer.cameraColorTargetHandle);  // use of target after allocation
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.isEnabled)
            return;

        cloudShadowsPass.settings = settings;
        renderer.EnqueuePass(cloudShadowsPass);
    }
    #else
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (!settings.isEnabled)
            return;

        cloudShadowsPass.settings = settings;
        cloudShadowsPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(cloudShadowsPass);
    }
    #endif
}
