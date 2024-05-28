using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
#endif
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

namespace Gaia
{
    public class SimpleCameraLayerCulling : MonoBehaviour
    {
        [HideInInspector]
        public GaiaSceneCullingProfile m_profile;

        public bool m_applyToGameCamera;
        public Light m_directionalLight;
        public Light m_moonLight;
        public bool m_applyToSceneCamera;

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            Initialize();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
#endif
        }

        public void Initialize()
        {
            if (m_profile == null)
            {
                GaiaSettings gaiaSettings = GaiaUtils.GetGaiaSettings();
                m_profile = ScriptableObject.CreateInstance<GaiaSceneCullingProfile>();
                m_profile.UpdateCulling(gaiaSettings);
                m_profile.UpdateShadow();
            }

            ApplyToGameCamera();
            ApplyToSceneCamera();
#if UNITY_EDITOR
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
#endif
        }

        private void OnCompilationFinished(object obj)
        {
            Refresh();
        }

        public void ApplyToGameCamera()
        {
            if (m_applyToGameCamera)
            {
                Camera cam = GetComponent<Camera>();
                if (cam != null)
                {
                    cam.layerCullDistances = m_profile.m_layerDistances;
                }
                ApplyToDirectionalLight();
            }
            else
            {
                float[] layerCulls = new float[32];
                for (int i = 0; i < layerCulls.Length; i++)
                {
                    layerCulls[i] = 0f;
                }
                Camera cam = GetComponent<Camera>();
                if (cam != null)
                {
                    cam.layerCullDistances = layerCulls;
                }
            }
        }

        public void ApplyToSceneCamera()
        {
#if UNITY_EDITOR
            if (m_applyToSceneCamera)
            {
                foreach (var sceneCamera in SceneView.GetAllSceneCameras())
                {
                    sceneCamera.layerCullDistances = m_profile.m_layerDistances;
                }
                ApplyToDirectionalLight();
            }
            else
            {
                float[] layerCulls = new float[32];
                for (int i = 0; i < layerCulls.Length; i++)
                {
                    layerCulls[i] = 0f;
                }
                foreach (var sceneCamera in SceneView.GetAllSceneCameras())
                {
                    sceneCamera.layerCullDistances = layerCulls;
                }
            }
#endif
        }

        public void ApplyToDirectionalLight()
        {
            if (m_directionalLight == null)
            {
                m_directionalLight = GaiaUtils.GetMainDirectionalLight(false);
            }

            if (m_directionalLight != null)
            {
                m_directionalLight.layerShadowCullDistances = m_profile.m_shadowLayerDistances;
            }

            if (m_moonLight == null)
            {
                m_moonLight = GaiaUtils.GetMainMoonLight(false);
            }

            if (m_moonLight != null)
            {
                m_moonLight.layerShadowCullDistances = m_profile.m_shadowLayerDistances;
            }
        }

        public void ResetDirectionalLight()
        {
            if (!m_applyToGameCamera && !m_applyToSceneCamera)
            {
                if (m_directionalLight == null)
                {
                    GameObject lightGO = GameObject.Find("Directional Light");
                    if (lightGO != null)
                    {
                        m_directionalLight = lightGO.GetComponent<Light>();
                    }

                    if (m_directionalLight != null)
                    {

                        float[] layers = new float[32];
                        for (int i = 0; i < layers.Length; i++)
                        {
                            layers[i] = 0f;
                        }
                        m_directionalLight.layerShadowCullDistances = layers;
                    }
                }
            }
        }

        public static void Refresh()
        {
            foreach (SimpleCameraLayerCulling sclc in GaiaUtils.FindOOTs<SimpleCameraLayerCulling>())
            {
                sclc.ApplyToGameCamera();
                sclc.ApplyToSceneCamera();
            }
        }
    }
}