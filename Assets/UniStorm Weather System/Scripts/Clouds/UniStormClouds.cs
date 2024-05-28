using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace UniStorm.Utility
{
    public class UniStormClouds : MonoBehaviour
    {
        [HideInInspector]
        public Material skyMaterial;
        [HideInInspector]
        public Material cloudsMaterial;
        [HideInInspector]
        public Material shadowsMaterial;
        [HideInInspector]
        public Material shadowsBuildingMaterial;
        [HideInInspector]
        public Transform cloudShadows;
        [HideInInspector]
        public Light sun;
        [HideInInspector]
        public Transform moon;
        [HideInInspector]
        public enum CloudPerformance { Low = 0, Medium = 1, High = 2, Ultra = 3 }
        [HideInInspector]
        private int[] presetResolutions = { 1024, 2048, 2048, 2048 };
        [HideInInspector]
        private string[] keywordsA = { "LOW", "MEDIUM", "HIGH", "ULTRA" };
        [HideInInspector]
        public enum CloudShadowsType { Off = 0, Simulated, RealTime }
        [HideInInspector]
        public CloudShadowsType CloudShadowsTypeRef = CloudShadowsType.Off;
        [HideInInspector]
        public enum CloudType { TwoD = 0, Volumetric}
        [HideInInspector]
        private string[] keywordsB = { "TWOD", "VOLUMETRIC" };
        [HideInInspector]
        public CloudType cloudType = CloudType.Volumetric;
        [HideInInspector]
        public CloudPerformance performance = CloudPerformance.High;
        [HideInInspector]
        public int CloudShadowResolutionValue = 256;
        [HideInInspector]
        [Range(0, 1)] public float cloudTransparency = 0.85f;
        [HideInInspector]
        [Range(0, 6)] public int shadowBlurIterations;
        [HideInInspector]
        public CommandBuffer cloudsCommBuff;
        //[HideInInspector]
        //public int numRendersPerFrame = 1;
        private int frameCount = 0;

        private void Start()
        {
            if (UniStormSystem.Instance.UseRuntimeDelay == UniStormSystem.EnableFeature.Enabled)
            {
                GenerateInitialNoise();
                StartCoroutine(InitializeClouds());
            }
            else
            {
                GenerateInitialNoise();
            }
        }

        // Generate the random noise needed for the clouds
        void GenerateInitialNoise()
        {
            SetCloudDetails(performance, cloudType, CloudShadowsTypeRef);
            GetComponent<MeshRenderer>().enabled = true;

            GenerateNoise.GenerateBaseCloudNoise();
            GenerateNoise.GenerateCloudDetailNoise();
            GenerateNoise.GenerateCloudCurlNoise();

            GetComponent<MeshFilter>().sharedMesh = ProceduralHemispherePolarUVs.hemisphere;
            GetComponentsInChildren<MeshFilter>()[1].sharedMesh = ProceduralHemispherePolarUVs.hemisphereInv;
            skyMaterial.SetFloat("_uLightningTimer", 0.0f);

            if (CloudShadowResolutionValue == 0)
            {
                CloudShadowResolutionValue = 256;
            }

            cloudsCommBuff = new CommandBuffer();
            cloudsCommBuff.name = "Render Clouds";

            shadowsBuildingMaterial = new Material(Shader.Find("Hidden/UniStorm/CloudShadows"));

            if (UniStormSystem.Instance.UseRuntimeDelay == UniStormSystem.EnableFeature.Disabled && UniStormSystem.Instance.PlayerCamera != null)
            {
                if (UniStormSystem.Instance.VRStateData.VREnabled && UniStormSystem.Instance.VRStateData.StereoRenderingMode == VRState.StereoRenderingModes.SinglePass)
                {
                    UniStormSystem.Instance.PlayerCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, cloudsCommBuff);
                }
                else
                {
                    UniStormSystem.Instance.PlayerCamera.AddCommandBuffer(CameraEvent.AfterSkybox, cloudsCommBuff);
                }
            }
            else if (UniStormSystem.Instance.UseRuntimeDelay == UniStormSystem.EnableFeature.Disabled && UniStormSystem.Instance.PlayerCamera == null)
            {
                StartCoroutine(InitializeClouds());
            }
        }

        IEnumerator InitializeClouds ()
        {          
            yield return new WaitUntil(()=> UniStormSystem.Instance.UniStormInitialized);

            if (UniStormSystem.Instance.VRStateData.VREnabled && UniStormSystem.Instance.VRStateData.StereoRenderingMode == VRState.StereoRenderingModes.SinglePass)
            {
                UniStormSystem.Instance.PlayerCamera.AddCommandBuffer(CameraEvent.BeforeImageEffects, cloudsCommBuff);
            }
            else
            {
                UniStormSystem.Instance.PlayerCamera.AddCommandBuffer(CameraEvent.AfterSkybox, cloudsCommBuff);
            }
        }

        #region Helper Functions and Variables
        public void EnsureArray<T>(ref T[] array, int size, T initialValue = default(T))
        {
            if (array == null || array.Length != size)
            {
                array = new T[size];
                for (int i = 0; i != size; i++)
                    array[i] = initialValue;
            }
        }

        public bool EnsureRenderTarget(ref RenderTexture rt, int width, int height, RenderTextureFormat format, FilterMode filterMode, string name, int depthBits = 0, int antiAliasing = 1)
        {
            if (rt != null && (rt.width != width || rt.height != height || rt.format != format || rt.filterMode != filterMode || rt.antiAliasing != antiAliasing))
            {
                RenderTexture.ReleaseTemporary(rt);
                rt = null;
            }
            if (rt == null)
            {
                rt = RenderTexture.GetTemporary(width, height, depthBits, format, RenderTextureReadWrite.Default, antiAliasing);
                rt.name = name;
                rt.filterMode = filterMode;
                rt.wrapMode = TextureWrapMode.Repeat;
                return true;// new target
            }

#if UNITY_ANDROID || UNITY_IPHONE
            rt.DiscardContents();
#endif

            return false;// same target
        }

        static int[] haltonSequence = {
            8, 4, 12, 2, 10, 6, 14, 1
        };

        static int[,] offset = {
                    {2,1}, {1,2 }, {2,0}, {0,1},
                    {2,3}, {3,2}, {3,1}, {0,3},
                    {1,0}, {1,1}, {3,3}, {0,0},
                    {2,2}, {1,3}, {3,0}, {0,2}
                };

        static int[,] bayerOffsets = {
            {0,8,2,10 },
            {12,4,14,6 },
            {3,11,1,9 },
            {15,7,13,5 }
        };
        #endregion

        private int frameIndex = 0;
        private int haltonSequenceIndex = 0;

        private int fullBufferIndex = 0;
        private RenderTexture[] fullCloudsBuffer;
        private RenderTexture lowResCloudsBuffer;
        private RenderTexture[] cloudShadowsBuffer;
        [HideInInspector]
        public RenderTexture PublicCloudShadowTexture;

        private float baseCloudOffset;
        private float detailCloudOffset;

        public void SetCloudDetails(CloudPerformance performance, CloudType cloudType, CloudShadowsType cloudShadowsType, bool forceRecreateTextures = false)
        {
            if (this.performance != performance || this.CloudShadowsTypeRef != cloudShadowsType || this.cloudType != cloudType
                || forceRecreateTextures)
            {
                if (cloudShadowsBuffer != null && cloudShadowsBuffer.Length > 0)
                {
                    cloudShadowsBuffer[0].Release();
                    cloudShadowsBuffer[1].Release();
                }
                if (lowResCloudsBuffer != null) lowResCloudsBuffer.Release();
                if (fullCloudsBuffer != null && fullCloudsBuffer.Length > 0)
                {
                    fullCloudsBuffer[0].Release();
                    fullCloudsBuffer[1].Release();
                }

                frameCount = 0;
            }

            this.performance = performance;
            this.cloudType = cloudType;
            this.CloudShadowsTypeRef = cloudShadowsType;

            switch (cloudShadowsType)
            {
                case CloudShadowsType.Off:
                    this.cloudShadows.gameObject.GetComponentsInChildren<MeshRenderer>()[1].enabled = (false);
                    sun.cookie = null;
                    break;
                case CloudShadowsType.Simulated:
                    this.cloudShadows.gameObject.GetComponentsInChildren<MeshRenderer>()[1].enabled = (false);
                    break;
                case CloudShadowsType.RealTime:
                    this.cloudShadows.gameObject.GetComponentsInChildren<MeshRenderer>()[1].enabled = (true);
                    sun.cookie = null;
                    break;
                default:
                    break;
            }

            foreach (string s in skyMaterial.shaderKeywords)
                skyMaterial.DisableKeyword(s);

            skyMaterial.EnableKeyword(keywordsA[(int)performance]);
            skyMaterial.EnableKeyword(keywordsB[(int)cloudType]);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            SetCloudDetails(performance, cloudType, CloudShadowsTypeRef, true);
        }
#endif

        void Update()
        {
            if (UniStormSystem.Instance.UniStormInitialized)
            {
                CloudsUpdate();
            }
        }

        void CloudsUpdate()
        {
            cloudShadows.position = UniStormSystem.Instance.PlayerCamera.transform.position;

            frameIndex = (frameIndex + 1) % 16;

            if (frameIndex == 0)
                haltonSequenceIndex = (haltonSequenceIndex + 1) % haltonSequence.Length;
            fullBufferIndex = fullBufferIndex ^ 1;

            float offsetX = offset[frameIndex, 0];
            float offsetY = offset[frameIndex, 1];

            frameCount++;
            if (frameCount < 32)
                skyMaterial.EnableKeyword("PREWARM");
            else if (frameCount == 32)
                skyMaterial.DisableKeyword("PREWARM");

            int size = presetResolutions[(int)performance];

            EnsureArray(ref fullCloudsBuffer, 2);
            EnsureArray(ref cloudShadowsBuffer, 2);
            EnsureRenderTarget(ref fullCloudsBuffer[0], size, size, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, "fullCloudBuff0");
            EnsureRenderTarget(ref fullCloudsBuffer[1], size, size, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, "fullCloudBuff1");
            EnsureRenderTarget(ref cloudShadowsBuffer[0], CloudShadowResolutionValue, CloudShadowResolutionValue, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, "cloudShadowBuff0");
            EnsureRenderTarget(ref cloudShadowsBuffer[1], size, size, RenderTextureFormat.ARGBHalf, FilterMode.Bilinear, "cloudShadowBuff1");

            EnsureRenderTarget(ref lowResCloudsBuffer, size / 4, size / 4, RenderTextureFormat.ARGBFloat, FilterMode.Point, "quarterCloudBuff");

            skyMaterial.SetTexture("_uBaseNoise", GenerateNoise.baseNoiseTexture);
            skyMaterial.SetTexture("_uDetailNoise", GenerateNoise.detailNoiseTexture);
            skyMaterial.SetTexture("_uCurlNoise", GenerateNoise.curlNoiseTexture);

            baseCloudOffset += skyMaterial.GetFloat("_uCloudsMovementSpeed") * Time.deltaTime;
            detailCloudOffset += skyMaterial.GetFloat("_uCloudsTurbulenceSpeed") * Time.deltaTime;

            skyMaterial.SetFloat("_uBaseCloudOffset", baseCloudOffset);
            skyMaterial.SetFloat("_uDetailCloudOffset", detailCloudOffset);

            skyMaterial.SetFloat("_uSize", size);
            skyMaterial.SetInt("_uCount", frameCount);
            skyMaterial.SetVector("_uJitter", new Vector2(offsetX, offsetY));
            skyMaterial.SetFloat("_uRaymarchOffset", (haltonSequence[haltonSequenceIndex] / 16.0f + bayerOffsets[offset[frameIndex, 0], offset[frameIndex, 1]] / 16.0f));

            skyMaterial.SetVector("_uSunDir", sun.transform.forward);
            skyMaterial.SetVector("_uMoonDir", Vector3.Normalize(moon.forward));
            skyMaterial.SetVector("_uWorldSpaceCameraPos", UniStormSystem.Instance.PlayerCamera.transform.position);

            #region Command Buffer
            cloudsCommBuff.Clear();

            // 1. Render the first clouds buffer - lower resolution
            cloudsCommBuff.Blit(null, lowResCloudsBuffer, skyMaterial, 0);

            // 2. Blend between low and hi-res
            cloudsCommBuff.SetGlobalTexture("_uLowresCloudTex", lowResCloudsBuffer);
            cloudsCommBuff.SetGlobalTexture("_uPreviousCloudTex", fullCloudsBuffer[fullBufferIndex]);
            cloudsCommBuff.Blit(fullCloudsBuffer[fullBufferIndex], fullCloudsBuffer[fullBufferIndex ^ 1], skyMaterial, 1);

            switch (CloudShadowsTypeRef)
            {
                case CloudShadowsType.Off:
                    break;
                case CloudShadowsType.Simulated:

                    // Low performance light cookie noise
                    shadowsBuildingMaterial.SetFloat("_uCloudsCoverage", skyMaterial.GetFloat("_uCloudsCoverage"));
                    shadowsBuildingMaterial.SetFloat("_uCloudsCoverageBias", skyMaterial.GetFloat("_uCloudsCoverageBias"));
                    shadowsBuildingMaterial.SetFloat("_uCloudsDensity", skyMaterial.GetFloat("_uCloudsDensity"));
                    shadowsBuildingMaterial.SetFloat("_uCloudsDetailStrength", skyMaterial.GetFloat("_uCloudsDetailStrength"));
                    shadowsBuildingMaterial.SetFloat("_uCloudsBaseEdgeSoftness", skyMaterial.GetFloat("_uCloudsBaseEdgeSoftness"));
                    shadowsBuildingMaterial.SetFloat("_uCloudsBottomSoftness", skyMaterial.GetFloat("_uCloudsBottomSoftness"));
                    shadowsBuildingMaterial.SetFloat("_uSimulatedCloudAlpha", cloudTransparency);

                    cloudsCommBuff.Blit(GenerateNoise.baseNoiseTexture, cloudShadowsBuffer[0], shadowsBuildingMaterial, 3);
                    PublicCloudShadowTexture = cloudShadowsBuffer[0];

                    break;
                case CloudShadowsType.RealTime:

                    cloudsCommBuff.Blit(fullCloudsBuffer[fullBufferIndex ^ 1], cloudShadowsBuffer[0]);
                    for (int i = 0; i < shadowBlurIterations; i++)
                    {
                        cloudsCommBuff.Blit(cloudShadowsBuffer[0], cloudShadowsBuffer[1], shadowsBuildingMaterial, 1);
                        cloudsCommBuff.Blit(cloudShadowsBuffer[1], cloudShadowsBuffer[0], shadowsBuildingMaterial, 2);
                    }

                    break;
                default:
                    break;
            }

            cloudsCommBuff.SetGlobalFloat("_uLightning", 0.0f);
            #endregion

            // 3. Set to material for the sky (not in the command buffer)
            cloudsMaterial.SetTexture("_MainTex", fullCloudsBuffer[fullBufferIndex ^ 1]);
        }
    }
}
 