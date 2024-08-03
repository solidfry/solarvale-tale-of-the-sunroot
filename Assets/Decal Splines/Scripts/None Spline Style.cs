using UnityEditor;

namespace DecalSplines
{
    //Class used to Hold Style info
    public class NoneSplineStyle : ISplineStyle
    {
#if UNITY_EDITOR
        public static NoneSplineStyle FindAsset()
        {
            string fileName = "No Selection";
            string searchFilter = $"\"{fileName}\" , t: {typeof(NoneSplineStyle).Name}";
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            NoneSplineStyle result = null;
            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                if (assetPath.Length > 0)
                {
                    result = (NoneSplineStyle)AssetDatabase.LoadAssetAtPath(assetPath, typeof(NoneSplineStyle));
                }
            }
            return result;
        }
#endif
    }
}
