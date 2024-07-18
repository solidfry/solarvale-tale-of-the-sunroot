using UnityEngine;
using UnityEditor;

using OccaSoftware.Buto.Runtime;

namespace OccaSoftware.Buto.Editor
{
    [CustomEditor(typeof(ButoLight))]
    [CanEditMultipleObjects]
    public class ButoLightEditor : UnityEditor.Editor
    {
        ButoLight _target;

        private static class Props
        {
            public static SerializedProperty inheritDataFromLightComponent;
            public static SerializedProperty lightColor;
            public static SerializedProperty lightIntensity;
            public static SerializedProperty lightComponent;
            public static SerializedProperty bias;
            public static SerializedProperty lightRange;
        }

        private void OnEnable()
        {
            Props.inheritDataFromLightComponent = serializedObject.FindProperty(nameof(Props.inheritDataFromLightComponent));
            Props.lightColor = serializedObject.FindProperty(nameof(Props.lightColor));
            Props.lightIntensity = serializedObject.FindProperty(nameof(Props.lightIntensity));
            Props.lightComponent = serializedObject.FindProperty(nameof(Props.lightComponent));
            Props.bias = serializedObject.FindProperty(nameof(Props.bias));
            Props.lightRange = serializedObject.FindProperty(nameof(Props.lightRange));

            _target = (ButoLight)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(Props.inheritDataFromLightComponent);
            if (Props.inheritDataFromLightComponent.boolValue)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(Props.lightComponent, new GUIContent("Light"));
                if (Props.lightComponent.objectReferenceValue != null)
                {
                    Light light = (Light)Props.lightComponent.objectReferenceValue;
                    EditorGUI.indentLevel++;
                    EditorGUILayout.ColorField("Color", light.color);
                    EditorGUILayout.FloatField("Intensity", light.intensity);
                    EditorGUILayout.FloatField("Range", light.range);
                    EditorGUI.indentLevel--;
                }

                EditorGUI.EndDisabledGroup();
            }

            if (!Props.inheritDataFromLightComponent.boolValue || Props.lightComponent.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(Props.lightColor, new GUIContent("Color"));
                EditorGUILayout.PropertyField(Props.lightIntensity, new GUIContent("Intensity"));
                EditorGUILayout.PropertyField(Props.lightRange, new GUIContent("Range"));
            }

            EditorGUILayout.PropertyField(Props.bias);

            if (Props.inheritDataFromLightComponent.boolValue && Props.lightComponent.objectReferenceValue == null)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Check or Add Light Component"))
                {
                    ForceGetLight();
                    _target.CheckForLight();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ForceGetLight()
        {
            if (!_target.TryGetComponent(out Light l))
            {
                l = Undo.AddComponent<Light>(_target.gameObject);
                ButoEditorCommon.SetupLight(l, Props.lightIntensity.floatValue, Props.lightIntensity.colorValue);
            }
        }
    }
}
