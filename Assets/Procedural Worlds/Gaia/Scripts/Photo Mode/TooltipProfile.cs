using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gaia
{
    [System.Serializable]
    public class TooltipProfileData
    {
        public string m_tooltipID;
        public string m_header;
        //[Multiline]
        [TextArea]
        public string m_text;
    }

    public class TooltipProfile : ScriptableObject
    {
        public List<TooltipProfileData> m_tooltips = new List<TooltipProfileData>();

        /// <summary>
        /// Create Tooltip Profile asset
        /// </summary>
#if UNITY_EDITOR
        [MenuItem("Assets/Create/Procedural Worlds/Gaia/Tooltip Profile")]
        public static void CreateTooltipProfile()
        {
            TooltipProfile asset = ScriptableObject.CreateInstance<TooltipProfile>();
            AssetDatabase.CreateAsset(asset, "Assets/Tooltip Profile.asset");
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
#endif
    }
}