#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FlatKit {
public static class SubAssetMaterial {
    public static Material GetOrCreate(Object settings, string shaderName) {
        // Return if called from OnValidate.
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var frames = stackTrace.GetFrames();
            if (frames != null) {
                if (frames.Select(frame => frame.GetMethod())
                    .Any(method => method != null && method.Name == "OnValidate")) {
                    return null;
                }
            }
        }

        var settingsPath = AssetDatabase.GetAssetPath(settings);
        var subAssets = AssetDatabase.LoadAllAssetsAtPath(settingsPath);
        const string subAssetName = "Effect Material";
        var existingMaterial = subAssets.FirstOrDefault(o => o.name == subAssetName) as Material;
        if (existingMaterial != null) return existingMaterial;

        var shader = Shader.Find(shaderName);
        if (shader == null) return null;

        var newMaterial = new Material(shader) { name = subAssetName };
        try {
            AssetDatabase.AddObjectToAsset(newMaterial, settings);
            AssetDatabase.ImportAsset(settingsPath);
        }
        catch {
            // ignored
        }

        return newMaterial;
    }

    public static void AlwaysInclude(string shaderName) {
        var shader = Shader.Find(shaderName);
        if (shader == null) return;
        var graphicsSettingsObj =
            AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        if (graphicsSettingsObj == null) return;
        var serializedObject = new SerializedObject(graphicsSettingsObj);
        var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
        bool hasShader = false;
        for (int i = 0; i < arrayProp.arraySize; ++i) {
            var arrayElem = arrayProp.GetArrayElementAtIndex(i);
            if (shader == arrayElem.objectReferenceValue) {
                hasShader = true;
                break;
            }
        }

        if (!hasShader) {
            int arrayIndex = arrayProp.arraySize;
            arrayProp.InsertArrayElementAtIndex(arrayIndex);
            var arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
            arrayElem.objectReferenceValue = shader;
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}
}
#endif