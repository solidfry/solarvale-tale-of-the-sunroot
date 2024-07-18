using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace OccaSoftware.Buto.Runtime
{
    [Serializable]
    public class VolumeNoise
    {
        Texture3D volumeTex;
        Material material;
        bool isDirty;

        public int frequency = 8;
        public int octaves = 4;
        public int lacunarity = 2;
        public float gain = 0.35f;
        public int seed = 0;
        public NoiseType noiseType = NoiseType.Perlin;
        public NoiseQuality noiseQuality = NoiseQuality.High;
        public bool invert = false;

        public Texture3D userTexture;

        int resolution
        {
            get
            {
                if (noiseType == NoiseType.None)
                    return 1;

                switch (noiseQuality)
                {
                    case NoiseQuality.Low:
                        return 16;
                    case NoiseQuality.Medium:
                        return 32;
                    case NoiseQuality.High:
                        return 64;
                    case NoiseQuality.Ultra:
                        return 128;
                    default:
                        return 32;
                }
            }
        }

        public enum NoiseQuality
        {
            Low,
            Medium,
            High,
            Ultra
        }

        public enum NoiseType
        {
            None,
            Texture,
            Perlin,
            Worley,
            PerlinWorley,
            Billow,
            Curl
        }

        /// <summary>
        /// Creates a new <see cref="VolumeNoise"/> asset.
        /// </summary>
        public VolumeNoise()
        {
            SetDirty();
        }

        public Texture3D GetTexture()
        {
            if (noiseType == NoiseType.Texture)
            {
                if (volumeTex != null)
                    Release();

                return userTexture;
            }

            userTexture = null;

            if (volumeTex == null)
            {
                volumeTex = new Texture3D(resolution, resolution, resolution, TextureFormat.RGBA32, true);
                volumeTex.name = "NoiseTexture";
                volumeTex.hideFlags = HideFlags.HideAndDontSave;
                volumeTex.wrapMode = TextureWrapMode.Repeat;
                isDirty = true;
            }

            if (isDirty)
            {
                if (noiseType == NoiseType.None)
                {
                    BakeEmpty();
                }
                else
                {
                    BakeTexture();
                }

                isDirty = false;
            }

            return volumeTex;
        }

        public void SetDirty()
        {
            isDirty = true;
        }

        private void BakeEmpty()
        {
            Color[] emptyColors = new Color[resolution * resolution * resolution];
            Array.Fill(emptyColors, Color.white);
            volumeTex.SetPixels(emptyColors);
            volumeTex.Apply(true);
        }

        private void BakeTexture()
        {
            RenderTexture rt = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear, 1);

            Shader s = Shader.Find("Hidden/Buto/VolumeNoiseShader");
            if (s == null)
            {
                Debug.Log("Can't find shader");
                return;
            }

            material = CoreUtils.CreateEngineMaterial(s);
            material.SetInt("_Frequency", frequency);
            material.SetInt("_Octaves", octaves);
            material.SetInt("_Lacunarity", lacunarity);
            material.SetFloat("_Gain", gain);
            material.SetInt("_Seed", seed);

            if (invert)
            {
                material.EnableKeyword("_INVERT_ON");
            }
            else
            {
                material.DisableKeyword("_INVERT_ON");
            }

            material.EnableKeyword("_GRAYSCALE_ON");

            Array enumValues = Enum.GetValues(typeof(NoiseType));
            foreach (NoiseType t in enumValues)
            {
                string keyword = "_TYPE_" + t.ToString().ToUpperInvariant();
                if (t == noiseType)
                {
                    material.EnableKeyword(keyword);
                }
                else
                {
                    material.DisableKeyword(keyword);
                }
            }

            Texture2D tempTex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, 0, true);
            RenderTexture.active = rt;
            int voxelCount = resolution * resolution * resolution;
            int sliceResolution = resolution * resolution;
            Color[] colors = new Color[voxelCount];

            for (int slice = 0; slice < resolution; slice++)
            {
                float h = (slice + 0.5f) / resolution;
                material.SetFloat("_Height", h);

                Graphics.Blit(null, rt, material);
                tempTex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
                Color[] sliceColors = tempTex.GetPixels();

                int sliceBaseIndex = slice * sliceResolution;
                for (int pixel = 0; pixel < sliceResolution; pixel++)
                {
                    colors[sliceBaseIndex + pixel] = sliceColors[pixel];
                }
            }

            CoreUtils.Destroy(tempTex);

            volumeTex.SetPixels(colors);
            volumeTex.Apply(true);

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);
            CoreUtils.Destroy(material);
        }

        public void Release()
        {
            if (noiseType != NoiseType.Texture && userTexture != null)
                userTexture = null;

            CoreUtils.Destroy(volumeTex);
            volumeTex = null;
        }
    }

    [Serializable]
    public class VolumeNoiseParameter : VolumeParameter<VolumeNoise>
    {
        public VolumeNoiseParameter(VolumeNoise value, bool overrideState = true)
            : base(value, overrideState) { }

        public override void Release() => m_Value.Release();
    }
}
