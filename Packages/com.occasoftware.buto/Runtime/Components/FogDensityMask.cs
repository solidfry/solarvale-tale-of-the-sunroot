using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace OccaSoftware.Buto.Runtime
{
    [ExecuteAlways]
    [AddComponentMenu("OccaSoftware/Buto/Fog Density Mask")]
    public sealed class FogDensityMask : ButoPlaceableObject
    {
        public enum PrimitiveShape
        {
            Sphere,
            Box
        }

        public enum BlendMode
        {
            Multiplicative,
            Exclusive
        }

        private Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        public Vector3 Size
        {
            get => Abs(transform.localScale);
        }

        [SerializeField]
        private PrimitiveShape shape = PrimitiveShape.Sphere;
        public PrimitiveShape Shape
        {
            get => shape;
        }

        [SerializeField]
        private BlendMode mode = BlendMode.Multiplicative;
        public BlendMode Mode
        {
            get { return mode; }
        }

        [SerializeField, Min(0)]
        private float densityMultiplier = 1;
        public float DensityMultiplier
        {
            get { return densityMultiplier; }
        }

        [SerializeField, Min(0)]
        private float blendDistance = 0;
        public float BlendDistance
        {
            get { return blendDistance; }
        }

        public static void SortByDistance(Vector3 c)
        {
            fogVolumes = fogVolumes.OrderBy(x => x.GetSqrMagnitude(c)).ToList();
        }

        private static List<FogDensityMask> fogVolumes = new List<FogDensityMask>();
        public static List<FogDensityMask> FogVolumes
        {
            get { return fogVolumes; }
        }

        protected override void Reset()
        {
            ButoCommon.CheckMaxFogVolumeCount(FogVolumes.Count, this);
        }

        protected override void OnEnable()
        {
            fogVolumes.Add(this);
        }

        protected override void OnDisable()
        {
            fogVolumes.Remove(this);
        }

        #region Editor
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (shape == PrimitiveShape.Sphere)
            {
                DrawSphere();
            }

            if (shape == PrimitiveShape.Box)
            {
                DrawBox();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (shape == PrimitiveShape.Sphere)
            {
                DrawSphereSelected();
            }

            if (shape == PrimitiveShape.Box)
            {
                DrawBoxSelected();
            }
        }

        private void DrawBoxSelected()
        {
            if (mode == BlendMode.Exclusive)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, Size);
                Gizmos.color = Color.red * new Color(1, 1, 1, 0.3f);
                Gizmos.DrawWireCube(transform.position, Size + Vector3.one * blendDistance);
            }

            if (mode == BlendMode.Multiplicative)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, Size);
                Gizmos.color = Color.green * new Color(1, 1, 1, 0.3f);
                Gizmos.DrawWireCube(transform.position, Size + Vector3.one * blendDistance);
            }
        }

        private void DrawSphereSelected()
        {
            if (mode == BlendMode.Exclusive)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, Size.x);
                Gizmos.color = Color.red * new Color(1, 1, 1, 0.3f);
                Gizmos.DrawWireSphere(transform.position, Size.x + blendDistance);
            }

            if (mode == BlendMode.Multiplicative)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, Size.x);
                Gizmos.color = Color.green * new Color(1, 1, 1, 0.3f);
                Gizmos.DrawWireSphere(transform.position, Size.x + blendDistance);
            }
        }

        private void DrawSphere()
        {
            if (mode == BlendMode.Exclusive)
            {
                Gizmos.color = Color.red * new Color(1, 1, 1, 0.1f);
                Gizmos.DrawSphere(transform.position, Size.x);
            }

            if (mode == BlendMode.Multiplicative)
            {
                Gizmos.color = Color.green * new Color(1, 1, 1, 0.1f);
                Gizmos.DrawSphere(transform.position, Size.x);
            }
        }

        private void DrawBox()
        {
            if (mode == BlendMode.Exclusive)
            {
                Gizmos.color = Color.red * new Color(1, 1, 1, 0.1f);
                Gizmos.DrawCube(transform.position, Size);
            }

            if (mode == BlendMode.Multiplicative)
            {
                Gizmos.color = Color.green * new Color(1, 1, 1, 0.1f);
                Gizmos.DrawCube(transform.position, Size);
            }
        }
#endif
        #endregion
    }
}
