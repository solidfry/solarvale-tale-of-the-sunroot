using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DecalSplines
{
    [CreateAssetMenu(fileName = "Spline Theme", menuName = "Decal Splines/Theme")]
    public class SplineTheme : ScriptableObject
    {
#if UNITY_EDITOR
        //List containing the styles of splines and the active index
        //Used to determine which style spline to place in the editor.
        [SerializeField] private List<ISplineStyle> styles;
        [SerializeField] private int styleIndex;

        public ISplineStyle ActiveStyle
        {
            get
            {
                if (styleIndex < styles.Count)
                    return styles[styleIndex];
                return null;
            }
        }

        public static SplineTheme[] GetAllInstances()
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(SplineTheme).Name);

            SplineTheme[] result = new SplineTheme[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                result[i] = (SplineTheme)AssetDatabase.LoadAssetAtPath(path, typeof(SplineTheme));
            }
            return result;
        }
#endif
    }
}
