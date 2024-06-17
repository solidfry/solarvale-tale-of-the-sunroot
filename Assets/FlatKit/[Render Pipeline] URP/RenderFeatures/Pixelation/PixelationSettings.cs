using System;
using ExternPropertyAttributes;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// ReSharper disable RedundantDefaultMemberInitializer

namespace FlatKit {
[CreateAssetMenu(fileName = "PixelationSettings", menuName = "FlatKit/Pixelation Settings")]
public class PixelationSettings : ScriptableObject {
    [Space] // Expandable.

#if !UNITY_2022_3_OR_NEWER
    [ExternPropertyAttributes.InfoBox(
        "Pixelation Effect requires Unity 2022.3 or newer. Please upgrade your Unity version to use this feature.",
        ExternPropertyAttributes.EInfoBoxType.Warning)]
    [Space]
#endif

    [Tooltip("The number of pixels on the longer side of the screen.\nLower values result in a more pixelated image.")]
    [Min(0)]
    public int resolution = 320;

    [HorizontalLine(1, EColor.Translucent)]
    [Tooltip("The render stage at which the effect is applied. To exclude transparent objects, like water or " +
             "UI elements, set this to \"Before Transparent\".")]
    public RenderPassEvent renderEvent = RenderPassEvent.BeforeRenderingPostProcessing;

    [Tooltip("Whether the effect should be applied in the Scene view as well as in the Game view. Please keep in " +
             "mind that Unity always renders the scene view with the default Renderer settings of the URP config.")]
    public bool applyInSceneView = true;

    [HideInInspector]
    public Material effectMaterial;

    internal Action onSettingsChanged;
    internal Action onReset;

    private void OnValidate() {
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