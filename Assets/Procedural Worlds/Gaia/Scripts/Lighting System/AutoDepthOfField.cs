using UnityEngine;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
using System;
#endif
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif
using UnityEngine.Rendering;

namespace Gaia
{
    /// <summary>
    /// A script to handle auto focus for Gaia
    /// </summary
    public class AutoDepthOfField : MonoBehaviour
    {
        #region Variables

        public bool m_disableSystem = false;
        public bool m_interactWithPlayer = true;
        public GaiaConstants.EnvironmentRenderer m_renderPipeLine = GaiaConstants.EnvironmentRenderer.BuiltIn;
        public GaiaConstants.DOFTrackingType m_trackingType = GaiaConstants.DOFTrackingType.FollowScreen;
        public float m_focusOffset = 0f;
        public Camera m_sourceCamera;
        public GameObject m_targetObject;
        public LayerMask m_targetLayer = 3585;
        public float m_maxFocusDistance = 100f;
        public float m_actualFocusDistance = 1f;
        public float m_dofAperture = 7.5f;
        public float m_dofFocalLength = 30f;
        public bool m_debug = false;
        public string m_playerTag = "Player";

        private bool m_maxDistanceExceeded = false;
        private GameObject m_debugSphere;
#if UNITY_POST_PROCESSING_STACK_V2
        private UnityEngine.Rendering.PostProcessing.DepthOfField m_dof;
#endif
#if UPPipeline
        private UnityEngine.Rendering.Universal.DepthOfField m_dofURP;
#endif
#if HDPipeline
        private UnityEngine.Rendering.HighDefinition.DepthOfField m_dofHDRP;
#endif

        /// <summary>
        /// Our last hit point
        /// </summary>
        private Vector3 m_dofTrackingPoint = Vector3.negativeInfinity;

        #endregion
        #region Unity Function

        /// <summary>
        /// Get the main camera if it doesnt exist
        /// </summary>
        private void Start()
        {
            SetupAutoFocus();
            GetCurrentDepthOfFieldSettings();
        }
        /// <summary>
        /// Apply on disable
        /// </summary>
        private void OnDisable()
        {
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                GaiaGlobal.Instance.SceneProfile.m_depthOfFieldFocusDistance = m_actualFocusDistance;
            }

            SetEditorDepthOfFieldSettings();
        }

        private void OnDestroy()
        {
            SetEditorDepthOfFieldSettings();
            SetDOFStatus(false);
        }

        /// <summary>
        /// Process DOF update
        /// </summary>
        private void Update()
        {
            if (m_disableSystem)
            {
                return;
            }

            switch (m_renderPipeLine)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_sourceCamera == null || m_dof == null)
                    {
                        return;
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_sourceCamera == null || m_dofURP == null)
                    {
                        return;
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (m_sourceCamera == null || m_dofHDRP == null)
                    {
                        return;
                    }
#endif
                    break;
                }
            }

            //Update the focus target
            UpdateDofTrackingPoint();
        }

        #endregion
        #region Depth Of Field Functions

        public void SetDOFMainSettings(float aperture, float focalLength)
        {
            switch (m_renderPipeLine)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_dof == null)
                    {
                        GetDepthOfFieldComponent();
                    }

                    if (m_dof != null)
                    {
                        m_dof.aperture.value = aperture;
                        m_dof.focalLength.value = focalLength;
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_dofURP == null)
                    {
                        GetDepthOfFieldComponent();
                    }

                    if (m_dofURP != null)
                    {
                        m_dofURP.aperture.value = aperture;
                        m_dofURP.focalLength.value = focalLength;
                    }
#endif
                    break;
                }
            }

        }
        /// <summary>
        /// Do a raycast to update the focus target
        /// </summary>
        private void UpdateDofTrackingPoint()
        {
            switch (m_trackingType)
            {
                case GaiaConstants.DOFTrackingType.LeftMouseClick:
                {
                    if (Input.GetMouseButton(0))
                    {
                        Ray ray = m_sourceCamera.ScreenPointToRay(Input.mousePosition);
                        FocusOnRayCast(ray);
                    }
                    break;
                }
                case GaiaConstants.DOFTrackingType.RightMouseClick:
                {
                    if (Input.GetMouseButton(1))
                    {
                        Ray ray = m_sourceCamera.ScreenPointToRay(Input.mousePosition);
                        FocusOnRayCast(ray);
                    }
                    break;
                }
                case GaiaConstants.DOFTrackingType.FollowScreen:
                {
                    Ray ray = new Ray(m_sourceCamera.transform.position, m_sourceCamera.transform.forward);
                    FocusOnRayCast(ray);
                    break;
                }
                case GaiaConstants.DOFTrackingType.FollowTarget:
                {
                    m_dofTrackingPoint = m_targetObject.transform.position;
                    break;
                }
                case GaiaConstants.DOFTrackingType.FixedOffset:
                {
                    if (m_debug)
                    {
                        m_debug = false;
                        Debug.Log("Debug mode is not available in Fixed Offset Tracking Mode. We have turned off Debug Mode for you.");
                    }
#if UNITY_POST_PROCESSING_STACK_V2
                    m_dof.focusDistance.value = m_focusOffset;
#endif
                    break;
                }
            }

            if (m_debug)
            {
                if (!Vector3.Equals(m_dofTrackingPoint,Vector3.negativeInfinity) && (m_debugSphere == null || m_debugSphere.transform.position != m_dofTrackingPoint))
                {
                    if (m_debugSphere != null)
                    {
                        Destroy(m_debugSphere);
                    }
                    m_debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(m_debugSphere.GetComponent<Collider>());
                    m_debugSphere.transform.position = m_dofTrackingPoint;
                    m_debugSphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                }
            }

            switch (m_renderPipeLine)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_trackingType != GaiaConstants.DOFTrackingType.FixedOffset)
                    {
                        SetNewDOFValue(m_dof.focusDistance.value);
                    }
                    m_actualFocusDistance = m_dof.focusDistance.value;
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_trackingType != GaiaConstants.DOFTrackingType.FixedOffset)
                    {
                        SetNewDOFValue(m_dofURP.focusDistance.value);
                    }
                    m_actualFocusDistance = m_dofURP.focusDistance.value;
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (m_trackingType != GaiaConstants.DOFTrackingType.FixedOffset)
                    {
                        SetNewDOFValue(m_dofHDRP.focusDistance.value);
                    }
                    m_actualFocusDistance = m_dofHDRP.focusDistance.value;
#endif
                    break;
                }
            }
            if (GaiaUtils.CheckIfSceneProfileExists())
            {
                GaiaGlobal.Instance.SceneProfile.m_depthOfFieldFocusDistance = m_actualFocusDistance;
            }

        }
        private void FocusOnRayCast(Ray ray)
        {
            if (Physics.Raycast(ray, out var hit, m_maxFocusDistance, m_targetLayer))
            {
                if (m_debug)
                {
                    Debug.Log("Auto DOF Ray collided with: " + hit.collider.name);
                }
                if (m_interactWithPlayer)
                {
                    if (!hit.transform.gameObject.CompareTag(m_playerTag))
                    {
                        m_maxDistanceExceeded = false;
                        m_dofTrackingPoint = hit.point;
                    }
                    else
                    {
                        m_maxDistanceExceeded = true;
                    }
                }
                else
                {
                    m_maxDistanceExceeded = false;
                    m_dofTrackingPoint = hit.point;
                }
            }
        }
        public void RemoveDebugSphere()
        {
            if (m_debugSphere != null)
            {
                DestroyImmediate(m_debugSphere);
            }
        }
        private void SetNewDOFValue(float startValue, float timeMultiplier = 3.5f)
        {
            if (m_sourceCamera == null)
            {
                return;
            }

            switch (m_renderPipeLine)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    if (m_dof == null)
                    {
                        GetDepthOfFieldComponent();
                    }
                    else
                    {
                        if (m_maxDistanceExceeded)
                        {
                            m_dof.focusDistance.value = m_maxFocusDistance;
                        }
                        else
                        {
                            m_dof.focusDistance.value = Mathf.Lerp(startValue, Vector3.Distance(m_sourceCamera.transform.position ,m_dofTrackingPoint) + m_focusOffset, Time.deltaTime * timeMultiplier);
                        }
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    if (m_dofURP == null)
                    {
                        GetDepthOfFieldComponent();
                    }
                    else
                    {
                        if (m_maxDistanceExceeded)
                        {
                            m_dofURP.focusDistance.value = m_maxFocusDistance;
                        }
                        else
                        {
                            m_dofURP.focusDistance.value = Mathf.Lerp(startValue, Vector3.Distance(m_sourceCamera.transform.position ,m_dofTrackingPoint) + m_focusOffset, Time.deltaTime * timeMultiplier);
                        }
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    if (m_dofHDRP == null)
                    {
                        GetDepthOfFieldComponent();
                    }
                    else
                    {
                        if (m_maxDistanceExceeded)
                        {
                            m_dofHDRP.focusDistance.value = m_maxFocusDistance;
                        }
                        else
                        {
                            m_dofHDRP.focusDistance.value = Mathf.Lerp(startValue, Vector3.Distance(m_sourceCamera.transform.position ,m_dofTrackingPoint) + m_focusOffset, Time.deltaTime * timeMultiplier);
                        }
                    }
#endif
                    break;
                }
            }

        }
        /// <summary>
        /// Setup autofocus object
        /// </summary>
        public void SetupAutoFocus()
        {
            m_renderPipeLine = GaiaUtils.GetActivePipeline();
            if (m_dofTrackingPoint.x > 0)
            {
                //Removes warning
            }

            if (m_maxDistanceExceeded)
            {
                //Removes warning
            }

            //Set up main camera
            if (m_sourceCamera == null)
            {
                m_sourceCamera = GaiaUtils.GetCamera();
                if (m_sourceCamera == null)
                {
                    Debug.Log("DOF Autofocus exiting, unable to find main camera!");
                    enabled = false;
                    return;
                }
            }

            //Determine tracking type
            if (m_trackingType == GaiaConstants.DOFTrackingType.FollowTarget && m_targetObject == null)
            {
                Debug.Log("Tracking target is missing, falling back to follow screen!");
                m_trackingType = GaiaConstants.DOFTrackingType.FollowScreen;
            }

            switch (m_renderPipeLine)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    //Gets DOF component
                    if (m_dof == null)
                    {
                        GetDepthOfFieldComponent();
                    }

                    if (m_dof != null)
                    {
                        m_dof.focusDistance.value = m_maxFocusDistance;
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    //Gets DOF component
                    if (m_dofURP == null)
                    {
                        GetDepthOfFieldComponent();
                    }

                    if (m_dofURP != null)
                    {
                        m_dofURP.focusDistance.value = m_maxFocusDistance;
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    //Gets DOF component
                    if (m_dofHDRP == null)
                    {
                        GetDepthOfFieldComponent();
                    }

                    if (m_dofHDRP != null)
                    {
                        m_dofHDRP.focusDistance.value = m_maxFocusDistance;
                    }
#endif
                    break;
                }
            }
        }

        private void GetDepthOfFieldComponent()
        {
            switch (m_renderPipeLine)
            {
                case GaiaConstants.EnvironmentRenderer.BuiltIn:
                {
#if UNITY_POST_PROCESSING_STACK_V2
                    //Find our DOF component
                    if (m_dof == null)
                    {
                        PostProcessProfile ppProfile = GaiaAPI.GetGlobalProcessingProfile();
                        if (ppProfile == null)
                        {
                            Debug.Log("DOF Autofocus exiting, unable to global post processing profile!");
                            enabled = false;
                            return;
                        }

                        if (!ppProfile.HasSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>())
                        {
                            Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                            enabled = false;
                            return;
                        }

                        if (!ppProfile.TryGetSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>(out m_dof))
                        {
                            Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                            m_dof = null;
                            enabled = false;
                        }
                    }
                    if (m_dof != null)
                    {
                        m_dof.enabled.value = true;
                        m_dof.focusDistance.overrideState = true;
                        m_maxDistanceExceeded = true;
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.Universal:
                {
#if UPPipeline
                    //Find our DOF component
                    if (m_dofURP == null)
                    {
                        VolumeProfile ppProfile = GaiaAPI.GetGlobalProcessingProfileURP();
                        if (ppProfile == null)
                        {
                            Debug.Log("DOF Autofocus exiting, unable to global post processing profile!");
                            enabled = false;
                            return;
                        }

                        if (!ppProfile.Has<UnityEngine.Rendering.Universal.DepthOfField>())
                        {
                            Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                            enabled = false;
                            return;
                        }

                        if (!ppProfile.TryGet<UnityEngine.Rendering.Universal.DepthOfField>(out m_dofURP))
                        {
                            Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                            m_dofURP = null;
                            enabled = false;
                        }
                    }
                    if (m_dofURP != null)
                    {
                        m_dofURP.active = true;
                        m_dofURP.focusDistance.overrideState = true;
                        m_maxDistanceExceeded = true;
                    }
#endif
                    break;
                }
                case GaiaConstants.EnvironmentRenderer.HighDefinition:
                {
#if HDPipeline
                    //Find our DOF component
                    if (m_dofHDRP == null)
                    {
                        VolumeProfile ppProfile = GaiaAPI.GetGlobalProcessingProfileHDRP();
                        if (ppProfile == null)
                        {
                            Debug.Log("DOF Autofocus exiting, unable to global post processing profile!");
                            enabled = false;
                            return;
                        }

                        if (!ppProfile.Has<UnityEngine.Rendering.HighDefinition.DepthOfField>())
                        {
                            Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                            enabled = false;
                            return;
                        }

                        if (!ppProfile.TryGet<UnityEngine.Rendering.HighDefinition.DepthOfField>(out m_dofHDRP))
                        {
                            Debug.Log("DOF Autofocus exiting, unable to find dof settings!");
                            m_dofHDRP = null;
                            enabled = false;
                        }
                    }
                    if (m_dofHDRP != null)
                    {
                        m_dofHDRP.active = true;
                        m_dofHDRP.focusDistance.overrideState = true;
                        m_maxDistanceExceeded = true;
                    }
#endif
                    break;
                }
            }
        }
        /// <summary>
        /// Sets the focus distance on the DOF component based on camera distance divided by 4
        /// </summary>
        /// <param name="enabled"></param>
        public void SetDOFStatus(bool enabled)
        {
            if (!enabled)
            {
#if UNITY_POST_PROCESSING_STACK_V2
                if (m_dof != null && m_sourceCamera != null)
                {
                    m_dof.focusDistance.value = m_sourceCamera.farClipPlane / 4f;
                }
#endif
            }
        }
        /// <summary>
        /// Grabs the DOF aperture and focal length and updates the systems values to use these
        /// </summary>
        public void GetCurrentDepthOfFieldSettings()
        {
            #if UNITY_POST_PROCESSING_STACK_V2

            if (m_dof != null)
            {
                m_dofAperture = m_dof.aperture.value;
                m_dofFocalLength = m_dof.focalLength.value;
            }

            #endif
        }
        /// <summary>
        /// Sets the depth of field focus distance to 100 if it's less than 20
        /// This should remove a blury game view when not in play mode
        /// </summary>
        private void SetEditorDepthOfFieldSettings()
        {
#if UNITY_POST_PROCESSING_STACK_V2
            if (m_dof == null)
            {
                return;
            }

            if (m_dof.focusDistance.value < 20f)
            {
                m_dof.focusDistance.value = 100f;
            }
#endif
        }

        #endregion
    }
}