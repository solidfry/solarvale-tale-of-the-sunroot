using System;
using ExternPropertyAttributes;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// ReSharper disable RedundantDefaultMemberInitializer

#if UNITY_2022_3_OR_NEWER
namespace FlatKit {
[CreateAssetMenu(fileName = "FogSettings", menuName = "FlatKit/Fog Settings")]
public class FogSettings : ScriptableObject {
    [Space] // Expandable.
    [Label("<b>Distance Fog</b>")]
    [Tooltip("Whether to use distance fog. This is the fog that fades out the scene into the background.")]
    public bool useDistance = true;

    [Tooltip("The color changes from near (left) to far (right).")]
    [Label("    Distance Gradient"), ShowIf(nameof(useDistance))]
    public Gradient distanceGradient;

    [Tooltip("The distance from the camera in world units at which the fog starts.")]
    [Label("    Near"), ShowIf(nameof(useDistance))]
    public float near = 0;

    [Tooltip("The distance from the camera in world units at which the fog ends.")]
    [Label("    Far"), ShowIf(nameof(useDistance))]
    public float far = 100;

    [Range(0, 1)]
    [Tooltip("How much the fog should be applied. 0 means no fog, 1 means full fog.")]
    [Label("    Intensity"), ShowIf(nameof(useDistance))]
    public float distanceFogIntensity = 1.0f;

    [Space(12)]
    [HorizontalLine(1, EColor.Translucent)]
    [Label("<b>Height Fog</b>")]
    [Tooltip("Whether to use height fog. This is the fog that goes up from the ground.")]
    public bool useHeight = false;

    [Tooltip("The color changes from low (left) to high (right).")]
    [Label("    Height Gradient"), ShowIf(nameof(useHeight))]
    public Gradient heightGradient;

    [Tooltip("The height in world units at which the fog starts.")]
    [Label("    Low"), ShowIf(nameof(useHeight))]
    public float low = 0;

    [Tooltip("The height in world units at which the fog ends.")]
    [Label("    High"), ShowIf(nameof(useHeight))]
    public float high = 10;

    [Range(0, 1)]
    [Tooltip("How much the fog should be applied. 0 means no fog, 1 means full fog.")]
    [Label("    Intensity"), ShowIf(nameof(useHeight))]
    public float heightFogIntensity = 1.0f;

    [Tooltip("Reverts fog behavior to pre-3.9.0. This is useful if you want to use the new fog settings, but want to " +
             "keep the old look of your scene.")]
    [Label("    Camera Relative Position"), ShowIf(nameof(useHeight))]
    public bool cameraRelativePosition = false;

    [Space(12)]
    [HorizontalLine]
    [Header("Blending")]
    [Range(0, 1)]
    [Tooltip("The ratio between distance and height fog. 0 means only distance fog, 1 means only height fog.")]
    [Label("    Distance/Height Blend")]
    public float distanceHeightBlend = 0.5f;

    [Header("Advanced Settings")]
    [Tooltip("The render stage at which the effect is applied. To exclude transparent objects, like water or " +
             "UI elements, set this to \"Before Transparent\".")]
    [Label("    Render Event")]
    public RenderPassEvent renderEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    [Tooltip("Whether the effect should be applied in the Scene view as well as in the Game view. Please keep in " +
             "mind that Unity always renders the scene view with the default Renderer settings of the URP config.")]
    [Label("    Apply In Scene View")]
    public bool applyInSceneView = true;

    [HideInInspector]
    public Material effectMaterial;

    internal Action onSettingsChanged;
    internal Action onReset;

    private void OnValidate() {
        low = Mathf.Min(low, high);
        high = Mathf.Max(low, high);

        onSettingsChanged?.Invoke();
    }

    private void Reset() {
        onReset?.Invoke();
    }

    private void OnDestroy() {
        onSettingsChanged = null;
        onReset = null;
    }
}
}
#else
namespace FlatKit {
[CreateAssetMenu(fileName = "FogSettings", menuName = "FlatKit/Fog Settings")]
public class FogSettings : ScriptableObject {
    [Header("Distance Fog")]
    public bool useDistance = true;
    public Gradient distanceGradient;
    public float near = 0;
    public float far = 100;

    [Range(0, 1)]
    public float distanceFogIntensity = 1.0f;
    public bool useDistanceFogOnSky = false;

    [Header("Height Fog")]
    [Space]
    public bool useHeight = false;
    public Gradient heightGradient;
    public float low = 0;
    public float high = 10;

    [Range(0, 1)]
    public float heightFogIntensity = 1.0f;
    public bool useHeightFogOnSky = false;

    [Header("Blending")]
    [Space]
    [Range(0, 1)]
    public float distanceHeightBlend = 0.5f;

    [Header("Advanced settings")]
    [Space, Tooltip("The render stage at which the effect is applied. To exclude transparent objects, like water or " +
                    "UI elements, set this to \"Before Transparent\".")]
    public RenderPassEvent renderEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    [Tooltip("Whether the effect should be applied in the Scene view as well as in the Game view. Please keep in " +
             "mind that Unity always renders the scene view with the default Renderer settings of the URP config.")]
    public bool applyInSceneView = true;

    private void OnValidate() {
        low = Mathf.Min(low, high);
        high = Mathf.Max(low, high);
    }
}
}
#endif