using UnityEngine;
using UnityEngine.Rendering.Universal;

#if UNITY_2022_3_OR_NEWER
using ExternPropertyAttributes;

namespace FlatKit {
public class FlatKitOutline : ScriptableRendererFeature {
    [Tooltip("To create new settings use 'Create > FlatKit > Outline Settings'.")]
    [Expandable]
    public OutlineSettings settings;

    private Material _effectMaterial;
    private DustyroomRenderPass _fullScreenPass;
    private bool _requiresColor;
    private bool _injectedBeforeTransparents;
    private ScriptableRenderPassInput _requirements = ScriptableRenderPassInput.Color;

    private const string ShaderName = "Hidden/FlatKit/OutlineWrap";
    private static int edgeColor => Shader.PropertyToID("_EdgeColor");
    private static int thickness => Shader.PropertyToID("_Thickness");
    private static int depthThresholdMin => Shader.PropertyToID("_DepthThresholdMin");
    private static int depthThresholdMax => Shader.PropertyToID("_DepthThresholdMax");
    private static int normalThresholdMin => Shader.PropertyToID("_NormalThresholdMin");
    private static int normalThresholdMax => Shader.PropertyToID("_NormalThresholdMax");
    private static int colorThresholdMin => Shader.PropertyToID("_ColorThresholdMin");
    private static int colorThresholdMax => Shader.PropertyToID("_ColorThresholdMax");
    private static int fadeRangeStart => Shader.PropertyToID("_FadeRangeStart");
    private static int fadeRangeEnd => Shader.PropertyToID("_FadeRangeEnd");

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

            _requirements = ScriptableRenderPassInput.Color; // Needed for the full-screen blit.
            if (settings.useDepth) _requirements |= ScriptableRenderPassInput.Depth;
            if (settings.useNormals) _requirements |= ScriptableRenderPassInput.Normal;
            if (settings.fadeWithDistance) _requirements |= ScriptableRenderPassInput.Depth;
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

        _fullScreenPass.Setup(_effectMaterial, _requiresColor, _injectedBeforeTransparents, "Flat Kit Outline",
            renderingData);
        renderer.EnqueuePass(_fullScreenPass);
    }

    protected override void Dispose(bool disposing) {
        _fullScreenPass?.Dispose();
    }

    private void SetMaterialProperties() {
        if (_effectMaterial == null) return;

        const string depthKeyword = "OUTLINE_USE_DEPTH";
        SetKeyword(_effectMaterial, depthKeyword, settings.useDepth);

        const string normalsKeyword = "OUTLINE_USE_NORMALS";
        SetKeyword(_effectMaterial, normalsKeyword, settings.useNormals);

        const string colorKeyword = "OUTLINE_USE_COLOR";
        SetKeyword(_effectMaterial, colorKeyword, settings.useColor);

        const string outlineOnlyKeyword = "OUTLINE_ONLY";
        SetKeyword(_effectMaterial, outlineOnlyKeyword, settings.outlineOnly);

        const string resolutionInvariantKeyword = "RESOLUTION_INVARIANT_THICKNESS";
        SetKeyword(_effectMaterial, resolutionInvariantKeyword, settings.resolutionInvariant);

        const string fadeWithDistanceKeyword = "OUTLINE_FADE_OUT";
        SetKeyword(_effectMaterial, fadeWithDistanceKeyword, settings.fadeWithDistance);

        _effectMaterial.SetColor(edgeColor, settings.edgeColor);
        _effectMaterial.SetFloat(thickness, settings.thickness);

        _effectMaterial.SetFloat(depthThresholdMin, settings.minDepthThreshold);
        _effectMaterial.SetFloat(depthThresholdMax, settings.maxDepthThreshold);

        _effectMaterial.SetFloat(normalThresholdMin, settings.minNormalsThreshold);
        _effectMaterial.SetFloat(normalThresholdMax, settings.maxNormalsThreshold);

        _effectMaterial.SetFloat(colorThresholdMin, settings.minColorThreshold);
        _effectMaterial.SetFloat(colorThresholdMax, settings.maxColorThreshold);

        _effectMaterial.SetFloat(fadeRangeStart, settings.fadeRangeStart);
        _effectMaterial.SetFloat(fadeRangeEnd, settings.fadeRangeEnd);
    }

    private static void SetKeyword(Material material, string keyword, bool enabled) {
        if (enabled) {
            material.EnableKeyword(keyword);
        } else {
            material.DisableKeyword(keyword);
        }
    }
}
}
#else
namespace FlatKit {
public class FlatKitOutline : ScriptableRendererFeature {
    [Tooltip("To create new settings use 'Create > FlatKit > Outline Settings'.")]
    public OutlineSettings settings;

    [SerializeField, HideInInspector]
    private Material _effectMaterial;

    private BlitTexturePass _blitTexturePass;

    private static readonly string OutlineShaderName = "Hidden/FlatKit/OutlineFilter";
    private static readonly int EdgeColor = Shader.PropertyToID("_EdgeColor");
    private static readonly int Thickness = Shader.PropertyToID("_Thickness");
    private static readonly int DepthThresholdMin = Shader.PropertyToID("_DepthThresholdMin");
    private static readonly int DepthThresholdMax = Shader.PropertyToID("_DepthThresholdMax");
    private static readonly int NormalThresholdMin = Shader.PropertyToID("_NormalThresholdMin");
    private static readonly int NormalThresholdMax = Shader.PropertyToID("_NormalThresholdMax");
    private static readonly int ColorThresholdMin = Shader.PropertyToID("_ColorThresholdMin");
    private static readonly int ColorThresholdMax = Shader.PropertyToID("_ColorThresholdMax");

    public override void Create() {
#if UNITY_EDITOR
        if (_effectMaterial == null) {
            SubAssetMaterial.AlwaysInclude(BlitTexturePass.CopyEffectShaderName);
            SubAssetMaterial.AlwaysInclude(OutlineShaderName);
        }
#endif

        if (settings == null) {
            return;
        }

        if (!CreateMaterials()) {
            return;
        }

        SetMaterialProperties();

        _blitTexturePass ??=
            new BlitTexturePass(_effectMaterial, settings.useDepth, settings.useNormals, useColor: true);
    }

    protected override void Dispose(bool disposing) {
        _blitTexturePass.Dispose();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
#if UNITY_EDITOR
        if (renderingData.cameraData.isPreviewCamera) return;
        if (!settings.applyInSceneView && renderingData.cameraData.cameraType == CameraType.SceneView) return;
#endif

        SetMaterialProperties();

        _blitTexturePass.Setup(renderingData);
        _blitTexturePass.renderPassEvent = settings.renderEvent;

        renderer.EnqueuePass(_blitTexturePass);
    }

    private bool CreateMaterials() {
        if (_effectMaterial == null) {
            var effectShader = Shader.Find(OutlineShaderName);
            var blitShader = Shader.Find(BlitTexturePass.CopyEffectShaderName);
            if (effectShader == null || blitShader == null) return false;
            _effectMaterial = UnityEngine.Rendering.CoreUtils.CreateEngineMaterial(effectShader);
        }

        return _effectMaterial != null;
    }

    private void SetMaterialProperties() {
        if (_effectMaterial == null) {
            return;
        }

        const string depthKeyword = "OUTLINE_USE_DEPTH";
        if (settings.useDepth) {
            _effectMaterial.EnableKeyword(depthKeyword);
        } else {
            _effectMaterial.DisableKeyword(depthKeyword);
        }

        const string normalsKeyword = "OUTLINE_USE_NORMALS";
        if (settings.useNormals) {
            _effectMaterial.EnableKeyword(normalsKeyword);
        } else {
            _effectMaterial.DisableKeyword(normalsKeyword);
        }

        const string colorKeyword = "OUTLINE_USE_COLOR";
        if (settings.useColor) {
            _effectMaterial.EnableKeyword(colorKeyword);
        } else {
            _effectMaterial.DisableKeyword(colorKeyword);
        }

        const string outlineOnlyKeyword = "OUTLINE_ONLY";
        if (settings.outlineOnly) {
            _effectMaterial.EnableKeyword(outlineOnlyKeyword);
        } else {
            _effectMaterial.DisableKeyword(outlineOnlyKeyword);
        }

        const string resolutionInvariantKeyword = "RESOLUTION_INVARIANT_THICKNESS";
        if (settings.resolutionInvariant) {
            _effectMaterial.EnableKeyword(resolutionInvariantKeyword);
        } else {
            _effectMaterial.DisableKeyword(resolutionInvariantKeyword);
        }

        _effectMaterial.SetColor(EdgeColor, settings.edgeColor);
        _effectMaterial.SetFloat(Thickness, settings.thickness);

        _effectMaterial.SetFloat(DepthThresholdMin, settings.minDepthThreshold);
        _effectMaterial.SetFloat(DepthThresholdMax, settings.maxDepthThreshold);

        _effectMaterial.SetFloat(NormalThresholdMin, settings.minNormalsThreshold);
        _effectMaterial.SetFloat(NormalThresholdMax, settings.maxNormalsThreshold);

        _effectMaterial.SetFloat(ColorThresholdMin, settings.minColorThreshold);
        _effectMaterial.SetFloat(ColorThresholdMax, settings.maxColorThreshold);
    }
}
}
#endif