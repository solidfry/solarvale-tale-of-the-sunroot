using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace OccaSoftware.Buto.Editor
{
    [InitializeOnLoad]
    public class AddCustomScriptingSymbols : MonoBehaviour
    {
        /// <summary>
        /// Symbols that will be added to the editor
        /// </summary>
        public static readonly string[] Symbols = new string[] { "OCCASOFTWARE", "BUTO" };

        /// <summary>
        /// Add define symbols as soon as Unity gets done compiling.
        /// </summary>
        static AddCustomScriptingSymbols()
        {
            string definesString = PlayerSettings.GetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup
            );
            List<string> allDefines = definesString.Split(';').ToList();
            allDefines.AddRange(Symbols.Except(allDefines));
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup,
                string.Join(";", allDefines.ToArray())
            );
        }
    }
}
