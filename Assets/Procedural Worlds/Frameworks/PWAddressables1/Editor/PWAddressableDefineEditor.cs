using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Compilation;
using UnityEngine.Rendering;

namespace ProceduralWorlds.Addressables1
{
    /// <summary>
    /// Injects GAIA_PRESENT define into project
    /// </summary>
    [InitializeOnLoad]
    public class PWAddressableDefineEditor : Editor
    {
        static PWAddressableDefineEditor()
        {

            bool updateScripting = false;
#if UNITY_2021_3_OR_NEWER
            string symbols = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup));
#else
            string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
#endif

            if (AddressablesPackageCheck())
            {
                if (!symbols.Contains("PW_ADDRESSABLES"))
                {
                    updateScripting = true;
                    symbols += ";PW_ADDRESSABLES";
                }
            }
            else
            {
                if (symbols.Contains("PW_ADDRESSABLES"))
                {
                    updateScripting = true;
                    symbols.Replace(";PW_ADDRESSABLES", "");
                    symbols.Replace("PW_ADDRESSABLES", "");
                }
            }

            if (updateScripting)
            {
#if UNITY_2021_3_OR_NEWER
                PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), symbols);
#else
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbols);
#endif
            }
        }

       
        /// <summary>
        /// Checks if the addressables package is installed via reflection
        /// </summary>
        /// <returns></returns>
        public static bool AddressablesPackageCheck()
        {
            //Look for assembly
            var assemblies = CompilationPipeline.GetAssemblies();
            foreach (UnityEditor.Compilation.Assembly assembly in assemblies)
            {
                if (assembly.name.Contains("Unity.Addressables"))
                {
                    //was found -> we are done
                    return true;
                }
            }
            return false;
        }

    }
}