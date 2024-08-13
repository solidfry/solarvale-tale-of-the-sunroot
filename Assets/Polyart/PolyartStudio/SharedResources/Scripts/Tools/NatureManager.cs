using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyart
{
    [ExecuteInEditMode]
    public class NatureManager : MonoBehaviour
    {
        [Header("Clouds")]
        [Tooltip("The X speed the clouds move with")]
        [Range(0f, 1f)]
        public float xSpeed = 0.2f;
        [Tooltip("The Y speed the clouds move with")]
        [Range(0f, 1f)]
        public float ySpeed = 0.2f;
        [Tooltip("The color of the cloud shadow")]
        public Color cloudColor = Color.white;

        [Header("Fading")]
        [Tooltip("Enable/Disable dithering")]
        public bool enableDithering = false;
        [Tooltip("The maximum distance the foliage is rendered")]
        [Range(0f, 200f)]
        public float fadingDistance = 30.0f;
        [Tooltip("Foliage/Terrain blending minimum")]
        [Range(-10f, 10f)]
        public float ditherBottomLevel = 1.0f;
        [Tooltip("Foliage/Terrain blending maximum")]
        [Range(-10f, 10f)]
        public float ditherFade = 6.0f;
        [Tooltip("Dithering when using default grass")]
        [Range(0f, 10f)]
        public float terrainGrassDitherMin = 1.0f;
        [Range(0f, 20f)]
        public float terrainGrassDitherMax = 6.0f;

        [Header("Wind")]
        [Tooltip("Small wind intensity")]
        [Range(0f, 100f)]
        public float smallWindIntensity = 20.0f;
        [Tooltip("Small wind multiplier")]
        [Range(-3f, 3f)]
        public float smallWindMultiplier = 1.0f;
        [Tooltip("Large wind intensity")]
        [Range(0f, 100f)]
        public float largeWindIntensity = 20.0f;
        [Tooltip("Large wind multiplier")]
        [Range(-3f, 3f)]
        public float largeWindMultiplier = 1.0f;

        [Header("Other")]
        [Tooltip("Small wind intensity")]
        [Range(0f, 1f)]
        public float smoothness = 0.1f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            if (enableDithering == true)
            {
                Shader.EnableKeyword("_DITHERINGON_ON");
            }
            else if (enableDithering == false)
            {
                Shader.DisableKeyword("_DITHERINGON_ON");
            }

            Vector2 cloudSpeed = new Vector2(xSpeed, ySpeed);
            Shader.SetGlobalVector("CloudSpeed", cloudSpeed);
            Shader.SetGlobalColor("CloudColor", cloudColor);
            Shader.SetGlobalFloat("FoliageRenderDistance", fadingDistance);
            Shader.SetGlobalFloat("DitherBottomLevel", ditherBottomLevel);
            Shader.SetGlobalFloat("DitherFade", ditherFade);
            Shader.SetGlobalFloat("FoliageDitherMin", terrainGrassDitherMin);
            Shader.SetGlobalFloat("FoliageDitherMax", terrainGrassDitherMax);
            Shader.SetGlobalFloat("WindNoiseSmall", smallWindIntensity);
            Shader.SetGlobalFloat("WindNoiseSmallMultiply", smallWindMultiplier);
            Shader.SetGlobalFloat("WindNoiseLarge", largeWindIntensity);
            Shader.SetGlobalFloat("WindNoiseLargeMultiply", largeWindMultiplier);
            Shader.SetGlobalFloat("FoliageSmoothness", smoothness);
        }

    }
}
