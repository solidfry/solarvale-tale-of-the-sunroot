using UnityEngine;
using UnityEditor;
using OccaSoftware.Buto.Runtime;

namespace OccaSoftware.Buto.Editor
{
    [CustomEditor(typeof(FogDensityMask)), CanEditMultipleObjects]
    public class FogDensityMaskEditor : UnityEditor.Editor
    {
        SerializedObject o;

        private static class Props
        {
            public static SerializedProperty shape;
            public static SerializedProperty mode;
            public static SerializedProperty densityMultiplier;
            public static SerializedProperty blendDistance;
        }

        private void OnEnable()
        {
            o = serializedObject;
            Props.shape = o.FindProperty(nameof(Props.shape));
            Props.mode = o.FindProperty(nameof(Props.mode));
            Props.densityMultiplier = o.FindProperty(nameof(Props.densityMultiplier));
            Props.blendDistance = o.FindProperty(nameof(Props.blendDistance));
        }

        public override void OnInspectorGUI()
        {
            o.Update();
            EditorGUILayout.PropertyField(Props.shape, new GUIContent("Shape"));
            EditorGUILayout.PropertyField(
                Props.mode,
                new GUIContent(
                    "Blend Mode",
                    "Set the blend mode of this fog mask. When set to Multiplicative, the mask will multiply the base fog density by the value set in Density Multiplier. When set to Exclusive, the mask will hide fog outside of its radius."
                )
            );
            bool isExclusiveMode = (FogDensityMask.BlendMode)Props.mode.enumValueIndex == FogDensityMask.BlendMode.Exclusive ? true : false;
            using (new EditorGUI.DisabledScope(isExclusiveMode))
            {
                EditorGUILayout.PropertyField(
                    Props.densityMultiplier,
                    new GUIContent(
                        "Density Multiplier",
                        "Set the multiplier for the base fog. Values below 1 decrease the fog; values above 1 increase the fog. Disabled when in Exclusive blend mode."
                    )
                );
            }

            EditorGUILayout.PropertyField(
                Props.blendDistance,
                new GUIContent(
                    "Blend Distance",
                    "Set the distance from the mask's edge at which mask will take full effect. For example, a value of 1 indicates that the mask will take full effect 1 unit outside of the mask bounds."
                )
            );

            o.ApplyModifiedProperties();
        }
    }
}
