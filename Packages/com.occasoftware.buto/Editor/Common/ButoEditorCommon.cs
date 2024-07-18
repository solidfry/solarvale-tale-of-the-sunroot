using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OccaSoftware.Buto.Editor
{
    internal static class ButoEditorCommon
    {
        internal static GameObject CreateChildAndSelect(string name, List<System.Type> types)
        {
            GameObject go = new GameObject(name, types.ToArray());
            go.transform.parent = Selection.activeTransform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            Selection.activeGameObject = go;
            return go;
        }

        internal static void SetupLight(Light l)
        {
            l.type = LightType.Point;
            l.bounceIntensity = 0;
            l.shadows = LightShadows.None;
        }

        internal static void SetupLight(Light l, float intensity, Color color)
        {
            l.type = LightType.Point;
            l.bounceIntensity = 0;
            l.shadows = LightShadows.None;
            l.intensity = intensity;
            l.color = color;
        }
    }
}
