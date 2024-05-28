#if UNITY_EDITOR
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#if !UNITY_2021_2_OR_NEWER
using UnityEngine.Experimental.TerrainAPI;
#endif
#endif

namespace Gaia
{
    public class NoiseComponent : MonoBehaviour
    {
        public Material mat;
        public NoiseSettings noiseSettings;

        void Update()
        {
            if(mat != null)
            {
                noiseSettings.SetupMaterial( mat );
            }
        }
    }
}
#endif