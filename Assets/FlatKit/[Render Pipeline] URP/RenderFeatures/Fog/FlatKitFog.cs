using UnityEngine;
using UnityEngine.Rendering.Universal;

#if UNITY_2022_3_OR_NEWER
using ExternPropertyAttributes;

namespace FlatKit {
public class FlatKitFog : ScriptableRendererFeature {
    [Tooltip("To create new settings use 'Create > FlatKit > Fog Settings'.")]
    [Expandable]
    public FogSettings settings;

    private Material _effectMaterial;
    private DustyroomRenderPass _fullScreenPass;
    private bool _requiresColor;
    private bool _injectedBeforeTransparents;
    private ScriptableRenderPassInput _requirements = ScriptableRenderPassInput.Color;

    private Texture2D _lutDepth;
    private Texture2D _lutHeight;

    private const string ShaderName = "Hidden/FlatKit/FogWrap";
    private const string CameraRelativePosition = "FOG_CAMERA_RELATIVE";
    private const string UseDistanceFog = "USE_DISTANCE_FOG";
    private const string UseHeightFog = "USE_HEIGHT_FOG";
    private static int distanceLut => Shader.PropertyToID("_DistanceLUT");
    private static int near => Shader.PropertyToID("_Near");
    private static int far => Shader.PropertyToID("_Far");
    private static int distanceFogIntensity => Shader.PropertyToID("_DistanceFogIntensity");
    private static int heightLut => Shader.PropertyToID("_HeightLUT");
    private static int lowWorldY => Shader.PropertyToID("_LowWorldY");
    private static int highWorldY => Shader.PropertyToID("_HighWorldY");
    private static int heightFogIntensity => Shader.PropertyToID("_HeightFogIntensity");
    private static int distanceHeightBlend => Shader.PropertyToID("_DistanceHeightBlend");

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

            _requirements = ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Color;
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

        _fullScreenPass.Setup(_effectMaterial, _requiresColor, _injectedBeforeTransparents, "Flat Kit Fog",
            renderingData);
        renderer.EnqueuePass(_fullScreenPass);
    }

    // Re-generate LUT textures when unity disposes them on scene save. 
#if UNITY_EDITOR
    public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData) {
        base.OnCameraPreCull(renderer, in cameraData);
        if (settings == null) return;
        if (settings.useDistance && !_effectMaterial.GetTexture(distanceLut)) UpdateDistanceLut();
        if (settings.useHeight && !_effectMaterial.GetTexture(heightLut)) UpdateHeightLut();
    }
#endif

    protected override void Dispose(bool disposing) {
        _fullScreenPass?.Dispose();
    }

    private void SetMaterialProperties() {
        if (_effectMaterial == null) return;

        SetKeyword(_effectMaterial, UseDistanceFog, settings.useDistance);
        if (settings.useDistance) {
            UpdateDistanceLut();
            _effectMaterial.SetFloat(near, settings.near);
            _effectMaterial.SetFloat(far, settings.far);
            _effectMaterial.SetFloat(distanceFogIntensity, settings.distanceFogIntensity);
        }

        SetKeyword(_effectMaterial, UseHeightFog, settings.useHeight);
        if (settings.useHeight) {
            UpdateHeightLut();
            _effectMaterial.SetFloat(lowWorldY, settings.low);
            _effectMaterial.SetFloat(highWorldY, settings.high);
            _effectMaterial.SetFloat(heightFogIntensity, settings.heightFogIntensity);
            _effectMaterial.SetFloat(distanceHeightBlend, settings.distanceHeightBlend);
        }

        SetKeyword(_effectMaterial, CameraRelativePosition, settings.cameraRelativePosition);
    }

    private void UpdateDistanceLut() {
        if (settings.distanceGradient == null) return;

        const int width = 256;
        const int height = 1;
        if (_lutDepth == null) {
            _lutDepth = new Texture2D(width, height, TextureFormat.RGBA32, /*mipChain=*/false) {
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear
            };
        }

        for (float x = 0; x < width; x++) {
            Color color = settings.distanceGradient.Evaluate(x / (width - 1));
            for (float y = 0; y < height; y++) {
                _lutDepth.SetPixel(Mathf.CeilToInt(x), Mathf.CeilToInt(y), color);
            }
        }

        _lutDepth.Apply();
        _effectMaterial.SetTexture(distanceLut, _lutDepth);
    }

    private void UpdateHeightLut() {
        if (settings.heightGradient == null) return;

        const int width = 256;
        const int height = 1;
        if (_lutHeight == null) {
            _lutHeight = new Texture2D(width, height, TextureFormat.RGBA32, /*mipChain=*/false) {
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Bilinear
            };
        }

        for (float x = 0; x < width; x++) {
            Color color = settings.heightGradient.Evaluate(x / (width - 1));
            for (float y = 0; y < height; y++) {
                _lutHeight.SetPixel(Mathf.CeilToInt(x), Mathf.CeilToInt(y), color);
            }
        }

        _lutHeight.Apply();
        _effectMaterial.SetTexture(heightLut, _lutHeight);
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
public class FlatKitFog : ScriptableRendererFeature {
    [Tooltip("To create new settings use 'Create > FlatKit > Fog Settings'.")]
    public FogSettings settings;

    [SerializeField, HideInInspector]
    private Material _effectMaterial;

    private BlitTexturePass _blitTexturePass;

    private Texture2D _lutDepth;
    private Texture2D _lutHeight;

    private static readonly string FogShaderName = "Hidden/FlatKit/FogFilter";
    private static readonly int DistanceLut = Shader.PropertyToID("_DistanceLUT");
    private static readonly int Near = Shader.PropertyToID("_Near");
    private static readonly int Far = Shader.PropertyToID("_Far");
    private static readonly int UseDistanceFog = Shader.PropertyToID("_UseDistanceFog");
    private static readonly int UseDistanceFogOnSky = Shader.PropertyToID("_UseDistanceFogOnSky");
    private static readonly int DistanceFogIntensity = Shader.PropertyToID("_DistanceFogIntensity");
    private static readonly int HeightLut = Shader.PropertyToID("_HeightLUT");
    private static readonly int LowWorldY = Shader.PropertyToID("_LowWorldY");
    private static readonly int HighWorldY = Shader.PropertyToID("_HighWorldY");
    private static readonly int UseHeightFog = Shader.PropertyToID("_UseHeightFog");
    private static readonly int UseHeightFogOnSky = Shader.PropertyToID("_UseHeightFogOnSky");
    private static readonly int HeightFogIntensity = Shader.PropertyToID("_HeightFogIntensity");
    private static readonly int DistanceHeightBlend = Shader.PropertyToID("_DistanceHeightBlend");

    public override void Create() {
#if UNITY_EDITOR
        if (_effectMaterial == null) {
            SubAssetMaterial.AlwaysInclude(BlitTexturePass.CopyEffectShaderName);
            SubAssetMaterial.AlwaysInclude(FogShaderName);
        }
#endif

        if (settings == null) {
            return;
        }

        if (!CreateMaterials()) {
            return;
        }

        SetMaterialProperties();

        _blitTexturePass = new BlitTexturePass(_effectMaterial, useDepth: true, useNormals: false, useColor: false);
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
            var effectShader = Shader.Find(FogShaderName);
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

        UpdateDistanceLut();
        _effectMaterial.SetTexture(DistanceLut, _lutDepth);
        _effectMaterial.SetFloat(Near, settings.near);
        _effectMaterial.SetFloat(Far, settings.far);
        _effectMaterial.SetFloat(UseDistanceFog, settings.useDistance ? 1f : 0f);
        _effectMaterial.SetFloat(UseDistanceFogOnSky, settings.useDistanceFogOnSky ? 1f : 0f);
        _effectMaterial.SetFloat(DistanceFogIntensity, settings.distanceFogIntensity);

        UpdateHeightLut();
        _effectMaterial.SetTexture(HeightLut, _lutHeight);
        _effectMaterial.SetFloat(LowWorldY, settings.low);
        _effectMaterial.SetFloat(HighWorldY, settings.high);
        _effectMaterial.SetFloat(UseHeightFog, settings.useHeight ? 1f : 0f);
        _effectMaterial.SetFloat(UseHeightFogOnSky, settings.useHeightFogOnSky ? 1f : 0f);
        _effectMaterial.SetFloat(HeightFogIntensity, settings.heightFogIntensity);
        _effectMaterial.SetFloat(DistanceHeightBlend, settings.distanceHeightBlend);
    }

    private void UpdateDistanceLut() {
        if (settings.distanceGradient == null) return;

        if (_lutDepth != null) {
            DestroyImmediate(_lutDepth);
        }

        const int width = 256;
        const int height = 1;
        _lutDepth = new Texture2D(width, height, TextureFormat.RGBA32, /*mipChain=*/false) {
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear
        };

        //22b5f7ed-989d-49d1-90d9-c62d76c3081a

        for (float x = 0; x < width; x++) {
            Color color = settings.distanceGradient.Evaluate(x / (width - 1));
            for (float y = 0; y < height; y++) {
                _lutDepth.SetPixel(Mathf.CeilToInt(x), Mathf.CeilToInt(y), color);
            }
        }

        _lutDepth.Apply();
    }

    private void UpdateHeightLut() {
        if (settings.heightGradient == null) return;

        if (_lutHeight != null) {
            DestroyImmediate(_lutHeight);
        }

        const int width = 256;
        const int height = 1;
        _lutHeight = new Texture2D(width, height, TextureFormat.RGBA32, /*mipChain=*/false) {
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear
        };

        for (float x = 0; x < width; x++) {
            Color color = settings.heightGradient.Evaluate(x / (width - 1));
            for (float y = 0; y < height; y++) {
                _lutHeight.SetPixel(Mathf.CeilToInt(x), Mathf.CeilToInt(y), color);
            }
        }

        _lutHeight.Apply();
    }
}
}
#endif