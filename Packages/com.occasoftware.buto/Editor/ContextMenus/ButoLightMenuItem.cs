using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using OccaSoftware.Buto.Runtime;

namespace OccaSoftware.Buto.Editor
{
    public class ButoLightMenuItem : EditorWindow
    {
        private static string objectName = "Buto Light";
        private static List<System.Type> types = new List<System.Type>() { typeof(Light), typeof(ButoLight) };

        [MenuItem("GameObject/Light/Buto Light")]
        public static void CreateButoLight()
        {
            GameObject go = ButoEditorCommon.CreateChildAndSelect(objectName, types);

            ButoEditorCommon.SetupLight(go.GetComponent<Light>());
            go.GetComponent<ButoLight>().SetInheritance(true);
        }
    }
}
