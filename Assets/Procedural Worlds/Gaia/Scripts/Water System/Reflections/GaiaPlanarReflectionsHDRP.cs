using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Gaia
{
    public enum HDRPWaterReflectionMode { PlanarReflection, ScreenSpaceReflection }

    [ExecuteAlways]
    public class GaiaPlanarReflectionsHDRP : MonoBehaviour
    {
        public static GaiaPlanarReflectionsHDRP Instance
        {
            get { return m_instance; }
        }
        [SerializeField] private static GaiaPlanarReflectionsHDRP m_instance;

        #region Public Variables

        public bool RequestRender
        {
            get { return m_requestRender; }
            set
            {
                if (m_requestRender != value)
                {
                    m_requestRender = value;
                    RequestReflectionRender();
                }
            }
        }
        public bool DisableCompletely
        {
            get { return m_disableCompletely; }
            set
            {
                if (m_disableCompletely != value)
                {
                    m_disableCompletely = value;
                    RefreshDisableCompletely();
                }
            }
        }
        public HDRPWaterReflectionMode ReflectionRenderMode
        {
            get { return m_reflectionRenderMode; }
            set
            {
                if (m_reflectionRenderMode != value)
                {
                    m_reflectionRenderMode = value;
                    switch (value)
                    {
                        case HDRPWaterReflectionMode.PlanarReflection:
                        {
                            DisableCompletely = false;
                            break;
                        }
                        case HDRPWaterReflectionMode.ScreenSpaceReflection:
                        {
                            DisableCompletely = true;
                            break;
                        }
                    }
                }
            }
        }

        public bool m_renderEveryFrame = true;
        public Camera m_mainCamera;
        public float m_reflectionIntenisty = 1f;
        public FrustumData m_frustumData = new FrustumData();
        public PositionCheckData m_positionCheckData = new PositionCheckData();
#if HDPipeline
        public PlanarReflectionProbe m_reflections;
#endif

        #endregion
        #region Private Variables

#if HDPipeline && UNITY_2020_2_OR_NEWER
        private int m_notInBoundsLayerMask = 0;
        private bool m_notInBoundsHasBeenSet = false;
        private bool m_isInMaskBounds = true;
        private LayerMask m_currentLayerMasks;
#endif
        [SerializeField] private Renderer m_waterPlane;
        [SerializeField] private bool m_requestRender;
        [SerializeField] private bool m_disableCompletely = false;
        [SerializeField] private HDRPWaterReflectionMode m_reflectionRenderMode = HDRPWaterReflectionMode.PlanarReflection;


        #endregion
        #region Unity Functions

        private void OnEnable()
        {
            m_instance = this;
#if HDPipeline && UNITY_2020_2_OR_NEWER
            if (m_reflections == null)
            {
                m_reflections = GetComponent<PlanarReflectionProbe>();
            }

            if (m_reflections != null)
            {
                m_reflections.realtimeMode = ProbeSettings.RealtimeMode.OnDemand;
                m_currentLayerMasks = m_reflections.settingsRaw.cameraSettings.culling.cullingMask;
                m_isInMaskBounds = true;
            }
#endif
            if (m_waterPlane == null)
            {
                PWS_WaterSystem waterSystem = PWS_WaterSystem.Instance;
                if (waterSystem != null)
                {
                    m_waterPlane = waterSystem.GetComponent<Renderer>();
                }
            }

            if (m_waterPlane != null)
            {
                m_frustumData.m_resultsCount = AdvancedWaterUtils.BuildOceanCullingGroup(m_mainCamera, m_waterPlane.transform.position.y, ref m_frustumData);
            }

            if (m_mainCamera == null)
            {
                m_mainCamera = GaiaUtils.GetCamera();
            }

#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            if (!Application.isPlaying)
            {
                EditorApplication.update += EditorUpdate;
            }
#endif
        }
        private void OnDisable()
        {
            if (m_frustumData.m_cullingGroup != null)
            {
                m_frustumData.m_cullingGroup.Dispose();
            }
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }
        private void OnDestroy()
        {
            if (m_frustumData.m_cullingGroup != null)
            {
                m_frustumData.m_cullingGroup.Dispose();
            }
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
#endif
        }
        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_renderEveryFrame && !DisableCompletely)
            {
                if (!m_positionCheckData.m_usePositionCheck)
                {
                    if (m_frustumData.m_useFrustumCalculation)
                    {
                        if (AdvancedWaterUtils.FrustumTest(m_frustumData))
                        {
                            RequestRender = true;
                        }
                    }
                    else
                    {
                        RequestRender = true;
                    }
                }
                else
                {
                    if (AdvancedWaterUtils.PositionChanged(m_mainCamera, ref m_positionCheckData))
                    {
                        if (m_frustumData.m_useFrustumCalculation)
                        {
                            if (AdvancedWaterUtils.FrustumTest(m_frustumData))
                            {
                                RequestRender = true;
                            }
                        }
                        else
                        {
                            RequestRender = true;
                        }
                    }
                }
            }
        }
        private void OnDrawGizmos()
        {
            AdvancedWaterUtils.DrawFrustumGizmos(m_frustumData, m_waterPlane);
        }

        #endregion
        #region Public Functions

        /// <summary>
        /// Request a render on the reflections at it's current state
        /// </summary>
        public void RequestReflectionRender()
        {
            m_requestRender = false;
#if HDPipeline && UNITY_2020_2_OR_NEWER
            if (m_reflections != null)
            {
                if (!m_isInMaskBounds)
                {
                    if (!m_notInBoundsHasBeenSet)
                    {
                        m_reflections.settingsRaw.cameraSettings.culling.cullingMask = m_notInBoundsLayerMask;
                        m_reflections.RequestRenderNextUpdate();
                        m_notInBoundsHasBeenSet = true;
                    }

                    return;
                }
                else
                {
                    if (m_notInBoundsHasBeenSet)
                    {
                        m_reflections.settingsRaw.cameraSettings.culling.cullingMask = m_currentLayerMasks;
                        m_notInBoundsHasBeenSet = false;
                    }

                    m_currentLayerMasks = m_reflections.settingsRaw.cameraSettings.culling.cullingMask;
                }

                m_reflections.RequestRenderNextUpdate();
            }
#endif
        }
        /// <summary>
        /// Sets the reflection state
        /// </summary>
        /// <param name="state"></param>
        public void SetReflectionState(bool state)
        {
#if HDPipeline && UNITY_2020_2_OR_NEWER
            m_isInMaskBounds = state;
#endif
        }
        /// <summary>
        /// Sets if the probe is enabled or not
        /// </summary>
        /// <param name="active"></param>
        public void ReflectionsActive(bool active, bool useDisableCompletely = true)
        {
#if HDPipeline && UNITY_2020_2_OR_NEWER
            if (m_reflections != null)
            {
                m_reflections.enabled = active;
                if (GaiaUtils.CheckIfSceneProfileExists(out SceneProfile sceneProfile))
                {
                    sceneProfile.m_enableReflections = active;
                }

                if (useDisableCompletely)
                {
                    if (active)
                    {
                        m_disableCompletely = false;
                    }
                    else 
                    {
                        m_disableCompletely = true;
                    }
                }
            }
#endif
        }
        /// <summary>
        /// Sets the reflection intensity
        /// </summary>
        /// <param name="newValue"></param>
        public void UpdateReflectionIntensity(float newValue)
        {
#if HDPipeline && UNITY_2020_2_OR_NEWER
            if (m_reflections != null)
            {
                m_reflections.settingsRaw.lighting.multiplier = newValue;
            }
#endif
        }
        /// <summary>
        /// Builds culling groups only used in editor only
        /// </summary>
        public int EditorBuildCullingGroup()
        {
            if (m_waterPlane == null)
            {
                PWS_WaterSystem waterSystem = PWS_WaterSystem.Instance;
                if (waterSystem != null)
                {
                    m_waterPlane = waterSystem.GetComponent<Renderer>();
                }
            }

            if (m_waterPlane != null)
            {
                m_frustumData.m_resultsCount = AdvancedWaterUtils.BuildOceanCullingGroup(m_mainCamera, m_waterPlane.transform.position.y, ref m_frustumData);
            }

            return m_frustumData.m_resultsCount;
        }

        #endregion
        #region Private Functions

        /// <summary>
        /// Refreshes the disabled state
        /// </summary>
        private void RefreshDisableCompletely()
        {
#if HDPipeline
            if (m_reflections != null)
            {
                if (DisableCompletely)
                {
                    m_reflections.enabled = false;
                }
                else
                {
                    m_reflections.enabled = true;
                }
            }
#endif
        }
        /// <summary>
        /// Editor Update
        /// </summary>
        private void EditorUpdate()
        {
            if (m_renderEveryFrame)
            {
                RequestRender = true;
            }
        }

        #endregion
    }
}