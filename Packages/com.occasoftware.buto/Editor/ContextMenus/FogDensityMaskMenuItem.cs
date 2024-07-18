using System.Collections.Generic;
using UnityEditor;

using OccaSoftware.Buto.Runtime;

namespace OccaSoftware.Buto.Editor
{
    public class FogDensityMaskMenuItem : EditorWindow
    {
        private static string objectName = "Buto Fog Density Mask";
        private static List<System.Type> types = new List<System.Type>() { typeof(FogDensityMask) };

        [MenuItem("GameObject/Rendering/Buto Fog Density Mask")]
        public static void CreateFogDensityMask()
        {
            ButoEditorCommon.CreateChildAndSelect(objectName, types);
        }
    }
}
