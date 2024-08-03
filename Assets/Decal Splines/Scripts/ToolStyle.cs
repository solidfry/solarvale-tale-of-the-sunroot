using UnityEditor;
using UnityEngine;

namespace DecalSplines
{
    public class EditorStyle : ScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] public Color PathColorDecal;
        [SerializeField] public Color PathColorMesh;
        [SerializeField] public Color PathColorNone;
        [SerializeField] public float PathWidth;
        [SerializeField] public Color PositionHandleColor;
        [SerializeField] public Color PositionHandleRemoveColor;
        [SerializeField] public float PositionHandleSize;
        [SerializeField] public Color CurveHandleColor;
        [SerializeField] public Color CurveHandleUnlockedColor;
        [SerializeField] public float CurveHandleSize;
        [SerializeField] public Color InsertHandleColor;
        [SerializeField] public float InsertHandleSize;
        [SerializeField] public bool ScreenSpaceHandles;
    }

    public static class EditorStyleUtility
    {
        static private EditorStyle _styles;
        static public EditorStyle Styles
        {
            get
            {
                if (_styles != null)
                    return _styles;
                else
                {
                    //Search for the style SO.
                    string fileName = "Decal Splines Style Config";
                    string searchFilter = $"\"{fileName}\"";
                    string[] guids = AssetDatabase.FindAssets(searchFilter);

                    if (guids.Length > 0)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        if (assetPath.Length > 0)
                        {
                            //find the template material
                            _styles = (EditorStyle)AssetDatabase.LoadAssetAtPath(assetPath, typeof(EditorStyle));
                        }
                    }
                }

                return _styles;
            }
        }
#endif
    }
}
