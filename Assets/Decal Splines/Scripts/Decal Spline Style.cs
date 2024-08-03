using UnityEngine;

namespace DecalSplines
{
    //Class used to Hold Style info
    [CreateAssetMenu(fileName = "New Spline Style", menuName = "Decal Splines/2D Decal")]
    public class DecalSplineStyle : ISplineStyle
    {
        [SerializeField] private Material material;
        [SerializeField] private float resolution;

        public Material Material { get { return material; } }
        public float Resolution { get { return resolution; } }

        public Vector2 GetTextureSize()
        {
            if (material != null)
            {
                Texture baseTex = material.GetTexture("Base_Map");
                if (baseTex != null)
                {
                    return new Vector2(baseTex.width, baseTex.height);
                }
            }
            return Vector2.one;
        }
    }
}

