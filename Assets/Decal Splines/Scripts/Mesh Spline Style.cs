using UnityEngine;

namespace DecalSplines
{
    //Class used to Hold Style info
    [CreateAssetMenu(fileName = "New Spline Style", menuName = "Decal Splines/3D Decal")]
    public class MeshSplineStyle : ISplineStyle
    {
        [SerializeField] private Transform prefab;

        public Transform Prefab { get { return prefab; } }

        public int BoneCount
        {
            get
            {
                int result = 0;
                foreach (Transform child in prefab)
                {
                    int i = 0;
                    while (child.Find("C" + i.ToString()) != null)
                    {
                        result++;
                        i++;
                    }
                    if (result > 0)
                        return result;
                }
                return result;
            }
        }
    }
}


