using FlatKit;
using UnityEditor;

namespace ExternPropertyAttributes.Editor {
[CanEditMultipleObjects]
[CustomEditor(typeof(OutlineSettings))]
public class OutlineSettingsInspector : ExternalCustomInspector { }

[CanEditMultipleObjects]
[CustomEditor(typeof(FogSettings))]
public class FogSettingsInspector : ExternalCustomInspector { }

[CanEditMultipleObjects]
[CustomEditor(typeof(PixelationSettings))]
public class PixelationSettingsInspector : ExternalCustomInspector { }
}