using ExternPropertyAttributes;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FlatKit {
public class FlatKitPixelation : ScriptableRendererFeature {
    [Expandable]
    [Tooltip("To create new settings use 'Create > FlatKit > Pixelation Settings'.")]
    public PixelationSettings settings;

    private Material _effectMaterial;
    private DustyroomRenderPass _fullScreenPass;
    private bool _requiresColor;
    private bool _injectedBeforeTransparents;
    private ScriptableRenderPassInput _requirements = ScriptableRenderPassInput.Color;

    private const string ShaderName = "Hidden/FlatKit/PixelationWrap";
    private static int pixelSizeProperty => Shader.PropertyToID("_PixelSize");

    public override void Create() {
        // Settings.
        {
            if (settings == null) return;
            settings.onSettingsChanged = null;
            settings.onReset = null;
            settings.onSettingsChanged += SetMaterialProperties;
            settings.onReset += SetMaterialProperties;
        }

        // Material.
        {
#if UNITY_EDITOR
            settings.effectMaterial = SubAssetMaterial.GetOrCreate(settings, ShaderName);
            if (settings.effectMaterial == null) return;
#endif
            _effectMaterial = settings.effectMaterial;
            SetMaterialProperties();
        }

        {
            _fullScreenPass = new DustyroomRenderPass {
                renderPassEvent = settings.renderEvent,
            };

            _requirements = ScriptableRenderPassInput.Color;
            ScriptableRenderPassInput modifiedRequirements = _requirements;

            _requiresColor = (_requirements & ScriptableRenderPassInput.Color) != 0;
            _injectedBeforeTransparents = settings.renderEvent <= RenderPassEvent.BeforeRenderingTransparents;

            if (_requiresColor && !_injectedBeforeTransparents) {
                modifiedRequirements ^= ScriptableRenderPassInput.Color;
            }

            _fullScreenPass.ConfigureInput(modifiedRequirements);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (settings == null || !settings.applyInSceneView && renderingData.cameraData.isSceneViewCamera) return;
        if (renderingData.cameraData.isPreviewCamera) return;
        if (_effectMaterial == null) return;

        _fullScreenPass.Setup(_effectMaterial, _requiresColor, _injectedBeforeTransparents, "Flat Kit Pixelation",
            renderingData);
        renderer.EnqueuePass(_fullScreenPass);
    }

    protected override void Dispose(bool disposing) {
        _fullScreenPass?.Dispose();
    }

    private void SetMaterialProperties() {
        if (_effectMaterial == null) return;
        var pixelSize = Mathf.Max(1f / settings.resolution, 0.0001f);
        _effectMaterial.SetFloat(pixelSizeProperty, pixelSize);
    }
}
}