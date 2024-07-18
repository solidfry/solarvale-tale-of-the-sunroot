using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OccaSoftware.Buto.Runtime
{
  public class ButoRenderFeature : ScriptableRendererFeature
  {
    class RenderFogPass : ScriptableRenderPass
    {
      private RTHandle source;
      private RenderTextureDescriptor descriptor;

      private RTHandle mergeTarget;
      private RTHandle isolatedBlitTarget;

      private ButoVolumetricFog volumetricFog;
      private Material mergeMaterial;
      private Material upscaleMaterial;
      private const string mergeShaderPath = "OccaSoftware/Buto/Merge";
      private const string isolatedBlitPath = "OccaSoftware/Buto/IsolatedBlit";

      private const string butoTextureId = "_ButoTexture";

      private const string passIdentifier = "Buto";

      private RenderTextureDescriptor GetVolumeDescriptor(Vector3Int volumeSize)
      {
        RenderTextureDescriptor volumeDescriptor = new RenderTextureDescriptor();
        volumeDescriptor.dimension = TextureDimension.Tex3D;
        volumeDescriptor.width = volumeSize.x;
        volumeDescriptor.height = volumeSize.y;
        volumeDescriptor.volumeDepth = volumeSize.z;
        volumeDescriptor.colorFormat = RenderTextureFormat.ARGBHalf;
        volumeDescriptor.enableRandomWrite = true;
        volumeDescriptor.msaaSamples = 1;
        return volumeDescriptor;
      }

      internal void SetupMaterials(Camera c)
      {
        GetShaderAndSetupMaterial(mergeShaderPath, ref mergeMaterial);
        GetShaderAndSetupMaterial(isolatedBlitPath, ref upscaleMaterial);

        if (mediaCompute == null)
        {
          mediaCompute = (ComputeShader)Resources.Load("MediaSampler");
        }

        if (integratorCompute == null)
        {
          integratorCompute = (ComputeShader)Resources.Load("Integrator");
        }

        if (lightingCompute == null)
        {
          lightingCompute = (ComputeShader)Resources.Load("LightingSampler");
        }

        Vector3Int volumeSize = volumetricFog.GetVolumeSize(
          new Vector2Int(c.pixelWidth, c.pixelHeight)
        );
        Vector3Int storedVolumeSize = Vector3Int.zero;

        if (kvps.TryGetValue(c, out RTData rt))
        {
          storedVolumeSize = rt.volumeSize;
        }
        else
        {
          rt = new RTData();
          kvps.Add(c, rt);
        }
        rt.refreshTextures = false;
        if (
          volumeSize != storedVolumeSize
          || rt.lightingDataRtPrev == null
          || rt.mediaDataRtPrev == null
        )
        {
          rt.refreshTextures = true;
        }

        mediaCompute.GetKernelThreadGroupSizes(
          0,
          out uint threadGroupSizeX,
          out uint threadGroupSizeY,
          out uint threadGroupSizeZ
        );

        groupsX = GetGroupCount(volumeSize.x, threadGroupSizeX);
        groupsY = GetGroupCount(volumeSize.y, threadGroupSizeY);
        groupsZ = GetGroupCount(volumeSize.z, threadGroupSizeZ);

        RenderTextureDescriptor volumeDescriptor = GetVolumeDescriptor(volumeSize);

        if (rt.refreshTextures)
        {
          if (rt.lightingDataRtPrev != null)
          {
            rt.lightingDataRtPrev.Release();
          }

          rt.lightingDataRtPrev = new RenderTexture(volumeDescriptor);
          rt.lightingDataRtPrev.name = "LightPrevRT_" + Random.Range(0, 1000);
          rt.lightingDataRtPrev.Create();
        }

        if (rt.refreshTextures)
        {
          if (rt.mediaDataRtPrev != null)
          {
            rt.mediaDataRtPrev.Release();
          }

          rt.mediaDataRtPrev = new RenderTexture(volumeDescriptor);
          rt.mediaDataRtPrev.name = "MediaPrevRT_" + Random.Range(0, 1000);
          rt.mediaDataRtPrev.Create();
        }

        rt.time = Time.realtimeSinceStartup;

        rt.volumeSize = volumeSize;
        rt.cellSize = volumetricFog.GetCellSize(volumeSize);
      }

      void GetShaderAndSetupMaterial(string path, ref Material target)
      {
        if (target != null)
          return;

        Shader s = Shader.Find(path);
        if (s != null)
        {
          target = CoreUtils.CreateEngineMaterial(s);
        }
        else
        {
          Debug.Log("Buto missing shader reference at " + path);
        }
      }

      Dictionary<Camera, RTData> kvps = new Dictionary<Camera, RTData>();

      private class RTData
      {
        public RenderTexture lightingDataRtPrev;
        public RenderTexture mediaDataRtPrev;
        public float time;
        public Matrix4x4 previousInverseTransform;
        public Vector3Int volumeSize;
        public Vector3 cellSize;
        public bool refreshTextures;
      }

      List<Camera> removeList = new List<Camera>();

      public void CleanupDictionary()
      {
        if (removeList.Count > 0)
          removeList.Clear();

        foreach (KeyValuePair<Camera, RTData> kvp in kvps)
        {
          if (kvp.Value.time - Time.realtimeSinceStartup > 0.5f)
          {
            removeList.Add(kvp.Key);
          }
        }

        foreach (Camera cam in removeList)
        {
          kvps.Remove(cam);
        }
      }

      ComputeShader mediaCompute;
      ComputeShader lightingCompute;
      ComputeShader integratorCompute;
      int groupsX;
      int groupsY;
      int groupsZ;

      private int GetGroupCount(int textureDimension, uint groupSize)
      {
        return Mathf.CeilToInt((textureDimension + groupSize - 1) / groupSize);
      }

      const string mergeTargetId = "Buto Final Merge Target";

      RTHandle mediaTarget;
      RTHandle lightingTarget;
      RTHandle integratorTarget;

      public RenderFogPass()
      {
        isolatedBlitTarget = RTHandles.Alloc(
          Shader.PropertyToID(butoTextureId),
          name: butoTextureId
        );
        mergeTarget = RTHandles.Alloc(Shader.PropertyToID(mergeTargetId), name: mergeTargetId);
        mediaTarget = RTHandles.Alloc("_MediaTarget", name: "_MediaTarget");
        lightingTarget = RTHandles.Alloc("_LightingTarget", name: "_LightingTarget");
        integratorTarget = RTHandles.Alloc("_IntegratorTarget", name: "_IntegratorTarget");
      }

      public bool RegisterStackComponent()
      {
        volumetricFog = VolumeManager.instance.stack.GetComponent<ButoVolumetricFog>();

        if (volumetricFog == null)
          return false;

        return volumetricFog.IsActive();
      }

      public void SetTarget(RTHandle colorHandle)
      {
        source = colorHandle;
      }

      public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
      {
        ConfigureTarget(source);

        descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.msaaSamples = 1;
        descriptor.depthBufferBits = 0;
        descriptor.colorFormat = RenderTextureFormat.DefaultHDR;
        descriptor.width = Mathf.Max(1, descriptor.width);
        descriptor.height = Mathf.Max(1, descriptor.height);

        RenderingUtils.ReAllocateIfNeeded(
          ref isolatedBlitTarget,
          descriptor,
          FilterMode.Point,
          TextureWrapMode.Clamp,
          name: butoTextureId
        );
        RenderingUtils.ReAllocateIfNeeded(
          ref mergeTarget,
          descriptor,
          FilterMode.Point,
          TextureWrapMode.Clamp,
          name: mergeTargetId
        );

        RenderTextureDescriptor volumeDescriptor = GetVolumeDescriptor(
          kvps[renderingData.cameraData.camera].volumeSize
        );

        RenderingUtils.ReAllocateIfNeeded(ref mediaTarget, volumeDescriptor);
        RenderingUtils.ReAllocateIfNeeded(ref lightingTarget, volumeDescriptor);
        RenderingUtils.ReAllocateIfNeeded(ref integratorTarget, volumeDescriptor);
      }

      Vector4[] volumePosition = new Vector4[ButoCommon._MAXVOLUMECOUNT];
      Vector4[] size = new Vector4[ButoCommon._MAXVOLUMECOUNT];
      float[] intensity = new float[ButoCommon._MAXVOLUMECOUNT];
      float[] blendMode = new float[ButoCommon._MAXVOLUMECOUNT];
      float[] blendDistance = new float[ButoCommon._MAXVOLUMECOUNT];
      float[] shape = new float[ButoCommon._MAXVOLUMECOUNT];

      Vector4[] lightPosition = new Vector4[ButoCommon._MAXLIGHTCOUNT];
      float[] lightStrength = new float[ButoCommon._MAXLIGHTCOUNT];
      Vector4[] color = new Vector4[ButoCommon._MAXLIGHTCOUNT];
      Vector4[] direction = new Vector4[ButoCommon._MAXLIGHTCOUNT];
      Vector4[] angle = new Vector4[ButoCommon._MAXLIGHTCOUNT];
      Vector4[] lightUp = new Vector4[ButoCommon._MAXLIGHTCOUNT];
      Vector4[] lightRight = new Vector4[ButoCommon._MAXLIGHTCOUNT];
      Vector4[] areaLightDimensions = new Vector4[ButoCommon._MAXLIGHTCOUNT];

      Vector4[] simpleColorSet = new Vector4[3];
      Matrix4x4 toPreviousView = Matrix4x4.identity;
      Vector3[] near = new Vector3[3];
      Vector3[] far = new Vector3[3];

      private void SetupCameraProperties(CommandBuffer cmd, Camera camera)
      {
        // Init profiler
        UnityEngine.Profiling.Profiler.BeginSample("Buto_Setup Camera Properties");

        // Calculate Frustum Sizes
        float nearClip = camera.nearClipPlane;
        float farClip = camera.farClipPlane;
        near[0] = camera.ViewportToWorldPoint(new Vector3(0, 0, nearClip));
        near[1] = camera.ViewportToWorldPoint(new Vector3(1, 0, nearClip));
        near[2] = camera.ViewportToWorldPoint(new Vector3(0, 1, nearClip));

        far[0] = camera.ViewportToWorldPoint(new Vector3(0, 0, farClip));
        far[1] = camera.ViewportToWorldPoint(new Vector3(1, 0, farClip));
        far[2] = camera.ViewportToWorldPoint(new Vector3(0, 1, farClip));

        Vector2 frustumNearSize =
          new Vector2(Vector3.Distance(near[1], near[0]), Vector3.Distance(near[2], near[0]))
          * 0.5f;

        Vector2 frustumFarSize =
          new Vector2(Vector3.Distance(far[1], far[0]), Vector3.Distance(far[2], far[0])) * 0.5f;

        // Setup fog sizes
        float fog_distance = volumetricFog.maxDistanceVolumetric.value;
        Vector2 fogFarSize = Vector2.Lerp(
          frustumNearSize,
          frustumFarSize,
          (fog_distance - nearClip) / (farClip - nearClip)
        );

        Vector2 fogNearSize = frustumNearSize;

        if (camera.orthographic)
        {
          fogNearSize = fogFarSize;
        }

        // Calculate To Previous View Matrix
        toPreviousView =
          kvps[camera].previousInverseTransform * camera.transform.localToWorldMatrix;

        // Set properties
        cmd.SetGlobalVector(
          Params.fog_volume_size.Id,
          new Vector4(
            kvps[camera].volumeSize.x,
            kvps[camera].volumeSize.y,
            kvps[camera].volumeSize.z,
            0
          )
        );
        cmd.SetGlobalVector("os_CameraForward", camera.transform.forward);
        cmd.SetGlobalVector(Params.fog_cell_size.Id, kvps[camera].cellSize);
        cmd.SetGlobalVector(Params.fogNearSize.Id, fogNearSize);
        cmd.SetGlobalVector(Params.fogFarSize.Id, fogFarSize);
        cmd.SetGlobalFloat(Params.cameraFarPlane.Id, camera.farClipPlane);
        cmd.SetGlobalFloat(Params._depth_ratio.Id, volumetricFog.depthRatio.value);
        cmd.SetGlobalFloat(Params.DensityInShadow.Id, volumetricFog.densityInShadow.value);
        cmd.SetGlobalFloat(Params.DensityInLight.Id, volumetricFog.densityInLight.value);
        cmd.SetGlobalMatrix(Params.toPreviousView.Id, toPreviousView);
        cmd.SetGlobalMatrix(
          Params.os_CameraInvProjection.Id,
          camera.nonJitteredProjectionMatrix.inverse
        );
        cmd.SetGlobalMatrix(Params.os_CameraToWorld.Id, camera.cameraToWorldMatrix);
        cmd.SetGlobalMatrix(Params.os_WorldToCamera.Id, camera.worldToCameraMatrix);
        cmd.SetGlobalVector(Params.os_WorldSpaceCameraPosition.Id, camera.transform.position);

        // Set Previous Inverse Transform
        kvps[camera].previousInverseTransform = camera.transform.worldToLocalMatrix;

        // Finish Profiler
        UnityEngine.Profiling.Profiler.EndSample();
      }

      Camera camera;

      public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
      {
        UnityEngine.Profiling.Profiler.BeginSample(passIdentifier);
        CommandBuffer cmd = CommandBufferPool.Get(passIdentifier);

        camera = renderingData.cameraData.camera;

        SetupData(cmd, camera);

        // Media Write
        cmd.SetGlobalTexture(Params._ScreenTexture.Id, source.nameID);
        RenderTargetIdentifier previousMediaData = new RenderTargetIdentifier(
          kvps[camera].mediaDataRtPrev
        );

        cmd.SetGlobalTexture(Params.MediaData.Id, mediaTarget);
        cmd.SetGlobalTexture(Params.MediaDataPrevious.Id, previousMediaData);
        cmd.DispatchCompute(mediaCompute, 0, groupsX, groupsY, groupsZ);
        if (
          mediaTarget.rt.width == kvps[camera].mediaDataRtPrev.width
          && mediaTarget.rt.height == kvps[camera].mediaDataRtPrev.height
          && mediaTarget.rt.volumeDepth == kvps[camera].mediaDataRtPrev.volumeDepth
        )
        {
          cmd.CopyTexture(mediaTarget, previousMediaData);
        }

        // Lighting write
        RenderTargetIdentifier previousLightingData = new RenderTargetIdentifier(
          kvps[camera].lightingDataRtPrev
        );
        cmd.SetGlobalTexture(Params.LightingDataTex.Id, lightingTarget);
        cmd.SetGlobalTexture(Params.LightingDataTexPrevious.Id, previousLightingData);
        cmd.DispatchCompute(lightingCompute, 0, groupsX, groupsY, groupsZ);
        if (
          lightingTarget.rt.width == kvps[camera].lightingDataRtPrev.width
          && lightingTarget.rt.height == kvps[camera].lightingDataRtPrev.height
          && lightingTarget.rt.volumeDepth == kvps[camera].lightingDataRtPrev.volumeDepth
        )
        {
          cmd.CopyTexture(lightingTarget, previousLightingData);
        }

        // Integrator write
        cmd.SetGlobalTexture(Params.IntegratorData.Id, integratorTarget);
        cmd.DispatchCompute(integratorCompute, 0, groupsX, groupsY, 1);

        // Write Buto only
        Blitter.BlitCameraTexture(cmd, source, isolatedBlitTarget, upscaleMaterial, 0);

        // Write to scene
        cmd.SetGlobalTexture(butoTextureId, isolatedBlitTarget.nameID);
        Blitter.BlitCameraTexture(cmd, source, mergeTarget, mergeMaterial, 0);

        // Blit back to camera
        Blitter.BlitCameraTexture(cmd, mergeTarget, source);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
        UnityEngine.Profiling.Profiler.EndSample();
      }

      public void Dispose()
      {
        mediaTarget?.Release();
        lightingTarget?.Release();
        integratorTarget?.Release();

        isolatedBlitTarget?.Release();
        mergeTarget?.Release();

        mediaTarget = null;
        lightingTarget = null;
        integratorTarget = null;
        isolatedBlitTarget = null;
        mergeTarget = null;

        kvps.Clear();
      }

      public override void FrameCleanup(CommandBuffer cmd)
      {
        CleanupDictionary();
      }

      private void SetupData(CommandBuffer cmd, Camera camera)
      {
        SetupCameraProperties(cmd, camera);
        SetupSphericalHarmonics(cmd);
        SetupMaterialData(cmd, camera);
        SetupAdditionalLightData(cmd, camera);
        SetupVolumeData(cmd, camera);
      }

      private void SetupMaterialData(CommandBuffer cmd, Camera cam)
      {
        UnityEngine.Profiling.Profiler.BeginSample("Buto_Setup Material Data");
        cmd.SetGlobalInt(Params.FrameId.Property, Time.frameCount);

        cmd.SetGlobalFloat(
          Params.MaxDistanceVolumetric.Id,
          volumetricFog.maxDistanceVolumetric.value
        );
        float inverseMaxDistance = Mathf.Max(
          1.0f / volumetricFog.maxDistanceVolumetric.value,
          1e-7f
        );
        float inverseDepthRatio = 1.0f / volumetricFog.depthRatio.value;
        cmd.SetGlobalFloat("_InverseMaxDistanceVolumetric", inverseMaxDistance);
        cmd.SetGlobalFloat("_inverse_depth_ratio", inverseDepthRatio);

        cmd.SetGlobalFloat(
          "_InverseLightDistanceVolumetric",
          volumetricFog.overrideDefaultMaxLightDistance.value
            ? Mathf.Max(1.0f / volumetricFog.maxLightDistance.value, 1e-7f)
            : inverseMaxDistance
        );

        WindOffsetHandler.UpdateWindOffset(volumetricFog.noiseWindSpeed.value);
        cmd.SetGlobalVector("buto_windOffset", WindOffsetHandler.position);

        cmd.SetGlobalFloat(
          "buto_TemporalIntegration_Lighting",
          GetIntegrationAmount(volumetricFog.temporalAALighting.value, cam)
        );
        cmd.SetGlobalFloat(
          "buto_TemporalIntegration_Media",
          GetIntegrationAmount(volumetricFog.temporalAAMedia.value, cam)
        );

        cmd.SetGlobalFloat(Params.Anisotropy.Id, volumetricFog.anisotropy.value);
        cmd.SetGlobalFloat(Params.BaseHeight.Id, volumetricFog.baseHeight.value);
        cmd.SetGlobalFloat(
          Params.AttenuationBoundarySize.Id,
          volumetricFog.attenuationBoundarySize.value
        );

        float density = volumetricFog.fogDensity.value * 0.01f;
        cmd.SetGlobalFloat(Params.FogDensity.Id, density);
        cmd.SetGlobalFloat(Params.LightIntensity.Id, volumetricFog.lightIntensity.value);

        cmd.SetGlobalFloat("buto_FrameId", Time.frameCount);
        cmd.SetGlobalTexture(
          Params.ColorRamp.Id,
          volumetricFog.colorRamp.value == null
            ? Texture2D.whiteTexture
            : volumetricFog.colorRamp.value
        );

        simpleColorSet[0] = (Vector4)volumetricFog.litColor.value;
        simpleColorSet[1] = (Vector4)volumetricFog.shadowedColor.value;
        simpleColorSet[2] = (Vector4)volumetricFog.emitColor.value;
        cmd.SetGlobalVectorArray(Params.SimpleColor.Id, simpleColorSet);
        cmd.SetGlobalFloat(Params.ColorInfluence.Id, volumetricFog.colorInfluence.value);

        texture = volumetricFog.volumeNoise.value.GetTexture();
        cmd.SetGlobalTexture(Params.NoiseTexture.Id, texture == null ? WhiteTexture : texture);

        cmd.SetGlobalInt(Params.Octaves.Id, volumetricFog.octaves.value);
        cmd.SetGlobalFloat(Params.NoiseTiling.Id, volumetricFog.noiseTiling.value);
        cmd.SetGlobalFloat(
          Params._InverseNoiseScale.Id,
          Mathf.Max(1.0f / volumetricFog.noiseTiling.value, 1e-7f)
        );
        cmd.SetGlobalFloat(
          Params._Inverse_AttenuationBoundarySize.Id,
          Mathf.Max(1.0f / volumetricFog.attenuationBoundarySize.value, 1e-7f)
        );

        cmd.SetGlobalVector(Params.NoiseWindSpeed.Id, volumetricFog.noiseWindSpeed.value);
        cmd.SetGlobalFloat(Params.NoiseIntensityMin.Id, volumetricFog.noiseMap.value.x);
        cmd.SetGlobalFloat(Params.NoiseIntensityMax.Id, volumetricFog.noiseMap.value.y);
        cmd.SetGlobalFloat(Params.Lacunarity.Id, volumetricFog.lacunarity.value);
        cmd.SetGlobalFloat(Params.Gain.Id, volumetricFog.gain.value);

        cmd.SetGlobalVector(
          Params.DirectionalLightingForward.Id,
          volumetricFog.directionalForward.value
        );
        cmd.SetGlobalVector(Params.DirectionalLightingBack.Id, volumetricFog.directionalBack.value);
        cmd.SetGlobalFloat(
          Params.DirectionalLightingRatio.Id,
          volumetricFog.directionalRatio.value
        );

        UnityEngine.Profiling.Profiler.EndSample();
      }

      private Texture3D texture;
      private Texture3D whiteTexture;

      private Texture3D WhiteTexture
      {
        get
        {
          if (whiteTexture == null)
          {
            whiteTexture = new Texture3D(1, 1, 1, TextureFormat.RGBA32, false);
            whiteTexture.SetPixel(0, 0, 0, Color.white);
            whiteTexture.Apply();
          }
          return whiteTexture;
        }
      }

      private float GetIntegrationAmount(float value, Camera cam)
      {
        if (Time.frameCount <= 2 || kvps[cam].refreshTextures)
          return 1.0f;

        return value;
      }

      void SetupAdditionalLightData(CommandBuffer cmd, Camera camera)
      {
        UnityEngine.Profiling.Profiler.BeginSample("Buto_Setup Additional Light Data");

        int lightCount = ButoLight.Lights.Count;
        cmd.SetGlobalInt(Params.LightCountButo.Id, lightCount);

        if (lightCount > 0)
        {
          if (lightCount > ButoCommon._MAXLIGHTCOUNT)
          {
            ButoLight.SortByDistance(camera.transform.position);
            lightCount = Mathf.Min(ButoLight.Lights.Count, ButoCommon._MAXLIGHTCOUNT);
          }

          for (int i = 0; i < lightCount; i++)
          {
            lightPosition[i] = ButoLight.Lights[i].LightPosition;
            lightStrength[i] = ButoLight.Lights[i].LightIntensity;
            color[i] = ButoLight.Lights[i].LightColor;
            direction[i] = ButoLight.Lights[i].LightDirection;
            angle[i] = ButoLight.Lights[i].LightAngleData.GetAngleData();
            lightUp[i] = ButoLight.Lights[i].transform.up;
            lightRight[i] = ButoLight.Lights[i].transform.right;
            areaLightDimensions[i] = ButoLight.Lights[i].GetAreaSize();
          }

          cmd.SetGlobalVectorArray(Params.LightPosButo.Id, lightPosition);
          cmd.SetGlobalFloatArray(Params.LightIntensityButo.Id, lightStrength);
          cmd.SetGlobalVectorArray(Params.LightColorButo.Id, color);
          cmd.SetGlobalVectorArray(Params.LightDirectionButo.Id, direction);
          cmd.SetGlobalVectorArray(Params.LightAngleButo.Id, angle);
          cmd.SetGlobalVectorArray("buto_LightUp", lightUp);
          cmd.SetGlobalVectorArray("buto_LightRight", lightRight);
          cmd.SetGlobalVectorArray("buto_AreaLightDimensions", areaLightDimensions);
        }

        UnityEngine.Profiling.Profiler.EndSample();
      }

      void SetupVolumeData(CommandBuffer cmd, Camera camera)
      {
        UnityEngine.Profiling.Profiler.BeginSample("Buto_Setup Volume Data");

        int volumeCount = FogDensityMask.FogVolumes.Count;
        cmd.SetGlobalInt(Params.VolumeCountButo.Id, volumeCount);

        if (volumeCount > 0)
        {
          if (volumeCount > ButoCommon._MAXVOLUMECOUNT)
          {
            FogDensityMask.SortByDistance(camera.transform.position);
            volumeCount = Mathf.Min(FogDensityMask.FogVolumes.Count, ButoCommon._MAXVOLUMECOUNT);
          }

          for (int i = 0; i < volumeCount; i++)
          {
            volumePosition[i] = (Vector4)FogDensityMask.FogVolumes[i].transform.position;
            size[i] = (Vector4)FogDensityMask.FogVolumes[i].Size;
            shape[i] = (int)FogDensityMask.FogVolumes[i].Shape;
            intensity[i] = FogDensityMask.FogVolumes[i].DensityMultiplier;
            blendMode[i] = (int)FogDensityMask.FogVolumes[i].Mode;
            blendDistance[i] = FogDensityMask.FogVolumes[i].BlendDistance;
          }

          cmd.SetGlobalVectorArray(Params.VolumePosition.Id, volumePosition);
          cmd.SetGlobalVectorArray(Params.VolumeSize.Id, size);
          cmd.SetGlobalFloatArray(Params.VolumeIntensity.Id, intensity);
          cmd.SetGlobalFloatArray(Params.VolumeBlendMode.Id, blendMode);
          cmd.SetGlobalFloatArray(Params.VolumeBlendDistance.Id, blendDistance);
          cmd.SetGlobalFloatArray(Params.VolumeShape.Id, shape);
        }

        UnityEngine.Profiling.Profiler.EndSample();
      }

      private void SetupSphericalHarmonics(CommandBuffer cmd)
      {
        UnityEngine.Profiling.Profiler.BeginSample("Buto_Setup Spherical Harmonics");
        SphericalHarmonicsL2 sh2 = RenderSettings.ambientProbe;
        Color ambient = new Color(
          sh2[0, 0] - sh2[0, 6],
          sh2[1, 0] - sh2[1, 6],
          sh2[2, 0] - sh2[2, 6]
        );
        cmd.SetGlobalColor(Params.WorldColor.Id, ambient);
        UnityEngine.Profiling.Profiler.EndSample();
      }
    }

    [System.Serializable]
    public class Settings
    {
      public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
    }

    public Settings settings = new Settings();

    RenderFogPass renderFogPass;

    public override void Create()
    {
      renderFogPass = new RenderFogPass();
      renderFogPass.renderPassEvent = settings.renderPassEvent;
    }

    private void OnDisable()
    {
      Shader.DisableKeyword("Buto");
      Shader.SetGlobalFloat(Params.ButoIsEnabled.Id, 0f);
    }

    public override void AddRenderPasses(
      ScriptableRenderer renderer,
      ref RenderingData renderingData
    )
    {
      Shader.SetGlobalFloat(Params.ButoIsEnabled.Id, 0f);
      Shader.DisableKeyword("Buto");
      if (renderingData.cameraData.camera.cameraType == CameraType.Reflection)
        return;

#if UNITY_EDITOR
      bool isSceneCamera = renderingData.cameraData.camera.cameraType == CameraType.SceneView;

      if (isSceneCamera)
      {
        bool fogEnabled = UnityEditor.SceneView.currentDrawingSceneView.sceneViewState.fogEnabled;

        bool isDrawingTextured =
          UnityEditor.SceneView.currentDrawingSceneView.cameraMode.drawMode
          == UnityEditor.DrawCameraMode.Textured;
        if (!fogEnabled || !isDrawingTextured)
          return;
      }

      if (renderingData.cameraData.camera.cameraType == CameraType.Preview)
        return;
#endif

      if (!renderingData.cameraData.postProcessEnabled)
        return;

      if (!renderFogPass.RegisterStackComponent())
        return;

      if (renderingData.cameraData.camera.TryGetComponent<DisableButoRendering>(out _))
        return;

      renderer.EnqueuePass(renderFogPass);
      Shader.SetGlobalFloat(Params.ButoIsEnabled.Id, 1f);
      Shader.EnableKeyword("Buto");
    }

    public override void SetupRenderPasses(
      ScriptableRenderer renderer,
      in RenderingData renderingData
    )
    {
      renderFogPass.RegisterStackComponent();
      renderFogPass.SetupMaterials(renderingData.cameraData.camera);
      renderFogPass.ConfigureInput(
        ScriptableRenderPassInput.Color
          | ScriptableRenderPassInput.Depth
          | ScriptableRenderPassInput.Motion
      );
      renderFogPass.SetTarget(renderer.cameraColorTargetHandle);
    }

    protected override void Dispose(bool disposing)
    {
      OnDisable();
      renderFogPass?.Dispose();
      renderFogPass = null;
      base.Dispose(disposing);
    }

    private static class Params
    {
      public readonly struct Param
      {
        public Param(string property)
        {
          Property = property;
          Id = Shader.PropertyToID(property);
        }

        readonly public string Property;
        readonly public int Id;
      }

      public static Param MediaData = new Param("MediaData");
      public static Param MediaDataPrevious = new Param("MediaDataPrevious");
      public static Param LightingDataTex = new Param("LightingDataTex");
      public static Param LightingDataTexPrevious = new Param("LightingDataTexPrevious");
      public static Param IntegratorData = new Param("IntegratorData");
      public static Param _ScreenTexture = new Param("_ScreenTexture");

      public static Param ButoIsEnabled = new Param("_ButoIsEnabled");
      public static Param FrameId = new Param("_FrameId");

      public static Param MaxDistanceVolumetric = new Param("_MaxDistanceVolumetric");
      public static Param MaxDistanceNonVolumetric = new Param("_MaxDistanceNonVolumetric");
      public static Param FogMaskBlendMode = new Param("_FogMaskBlendMode");

      // TAA Param
      public static Param TemporalAaIntegrationRate = new Param("_IntegrationRate");

      public static Param FogDensity = new Param("_FogDensity");
      public static Param Anisotropy = new Param("_Anisotropy");
      public static Param LightIntensity = new Param("_LightIntensity");
      public static Param DensityInShadow = new Param("_DensityInShadow");
      public static Param DensityInLight = new Param("_DensityInLight");

      public static Param BaseHeight = new Param("_BaseHeight");
      public static Param AttenuationBoundarySize = new Param("_AttenuationBoundarySize");

      public static Param ColorRamp = new Param("_ColorRamp");
      public static Param SimpleColor = new Param("_SimpleColor");
      public static Param ColorInfluence = new Param("_ColorInfluence");

      public static Param NoiseTexture = new Param("_NoiseTexture");
      public static Param Octaves = new Param("_Octaves");
      public static Param Lacunarity = new Param("_Lacunarity");
      public static Param Gain = new Param("_Gain");
      public static Param NoiseTiling = new Param("_NoiseTiling");
      public static Param NoiseWindSpeed = new Param("_NoiseWindSpeed");
      public static Param NoiseIntensityMin = new Param("_NoiseIntensityMin");
      public static Param NoiseIntensityMax = new Param("_NoiseIntensityMax");

      public static Param LightCountButo = new Param("_LightCountButo");
      public static Param LightPosButo = new Param("_LightPosButo");
      public static Param LightIntensityButo = new Param("_LightIntensityButo");
      public static Param LightColorButo = new Param("_LightColorButo");
      public static Param LightDirectionButo = new Param("_LightDirectionButo");
      public static Param LightAngleButo = new Param("_LightAngleButo");

      // Volume Data
      public static Param VolumeCountButo = new Param("_VolumeCountButo");
      public static Param VolumePosition = new Param("_VolumePosition");
      public static Param VolumeSize = new Param("_VolumeSize");
      public static Param VolumeShape = new Param("_VolumeShape");
      public static Param VolumeIntensity = new Param("_VolumeIntensityButo");
      public static Param VolumeBlendMode = new Param("_VolumeBlendMode");
      public static Param VolumeBlendDistance = new Param("_VolumeBlendDistance");

      public static Param DirectionalLightingForward = new Param("_DirectionalLightingForward");
      public static Param DirectionalLightingBack = new Param("_DirectionalLightingBack");
      public static Param DirectionalLightingRatio = new Param("_DirectionalLightingRatio");

      // Ambient Lighting
      public static Param WorldColor = new Param("_WorldColor");

      public static Param fog_volume_size = new Param("fog_volume_size");
      public static Param fog_cell_size = new Param("fog_cell_size");
      public static Param fogNearSize = new Param("fogNearSize");
      public static Param fogFarSize = new Param("fogFarSize");
      public static Param cameraFarPlane = new Param("cameraFarPlane");
      public static Param _depth_ratio = new Param("_depth_ratio");
      public static Param toPreviousView = new Param("toPreviousView");
      public static Param os_CameraInvProjection = new Param("os_CameraInvProjection");
      public static Param os_CameraToWorld = new Param("os_CameraToWorld");
      public static Param os_WorldToCamera = new Param("os_WorldToCamera");
      public static Param os_WorldSpaceCameraPosition = new Param("os_WorldSpaceCameraPosition");

      public static Param _InverseNoiseScale = new Param("_InverseNoiseScale");
      public static Param _Inverse_AttenuationBoundarySize = new Param(
        "_Inverse_AttenuationBoundarySize"
      );
    }
  }
}
