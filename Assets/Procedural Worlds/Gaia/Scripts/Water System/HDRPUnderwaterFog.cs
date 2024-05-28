using UnityEngine;
#if HDPipeline
using UnityEngine.Rendering.HighDefinition;
#endif

namespace Gaia
{
    [System.Serializable]
    public class UnderwaterFogProfile
    {
        public Color m_albedo = new Color(0.1780883f, 0.3396979f, 0.5471698f, 1f);
        public float m_fogDistance = 2.5f;
        public Vector3 m_size = new Vector3(1000f, 1000f, 1000f);
        public float m_distanceFadeStart = 10000f;
        public float m_distanceFadeEnd = 10000f;
        public Texture3D m_texture;
        public Vector3 m_scrollSpeed = new Vector3(0.1f, 0f, 0.12f);
        public Vector3 m_tiling = new Vector3(256, 256, 256);

#if HDPipeline
        /// <summary>
        /// Applies the latest settings to the density volume
        /// </summary>
        /// <param name="densityVolume"></param>
        #if UNITY_2021_2_OR_NEWER
        public void Apply(LocalVolumetricFog densityVolume)
        #else
        public void Apply(DensityVolume densityVolume)
        #endif
        {
            if (densityVolume != null)
            {
                densityVolume.parameters.albedo = m_albedo;
                densityVolume.parameters.meanFreePath = m_fogDistance;
                densityVolume.parameters.size = m_size;
                densityVolume.parameters.distanceFadeStart = m_distanceFadeStart;
                densityVolume.parameters.distanceFadeEnd = m_distanceFadeEnd;
                densityVolume.parameters.volumeMask = m_texture;
                densityVolume.parameters.textureScrollingSpeed = m_scrollSpeed;
                densityVolume.parameters.textureTiling = m_tiling;
            }
        }
        /// <summary>
        /// Compares the settings and returns true if one of the settings doesn't match
        /// </summary>
        /// <param name="compareProfile"></param>
        /// <returns></returns>
        public bool Compare(UnderwaterFogProfile compareProfile)
        {
            if (compareProfile.m_albedo != m_albedo)
            {
                return true;
            }
            if (compareProfile.m_fogDistance != m_fogDistance)
            {
                return true;
            }
            if (compareProfile.m_size != m_size)
            {
                return true;
            }
            if (compareProfile.m_distanceFadeStart != m_distanceFadeStart)
            {
                return true;
            }
            if (compareProfile.m_distanceFadeEnd != m_distanceFadeEnd)
            {
                return true;
            }
            if (compareProfile.m_texture != m_texture)
            {
                return true;
            }
            if (compareProfile.m_tiling != m_tiling)
            {
                return true;
            }

            return false;
        }
#endif
    }

    public class HDRPUnderwaterFog : MonoBehaviour
    {
#if HDPipeline
        public UnderwaterFogProfile UnderwaterProfile
        {
            get { return m_underwaterProfile; }
            set
            {
                if (m_underwaterProfile.Compare(value))
                {
                    m_underwaterProfile = value;
                    m_underwaterProfile.Apply(m_densityVolume);
                }
            }
        }
        [SerializeField]
        private UnderwaterFogProfile m_underwaterProfile = new UnderwaterFogProfile();
#if UNITY_2021_2_OR_NEWER
        public LocalVolumetricFog m_densityVolume;
#else
		public DensityVolume m_densityVolume;
#endif
        [SerializeField]
        private GaiaGlobal m_gaiaGlobal;
        [SerializeField]
        private Camera m_camera;
        [SerializeField]
        private GaiaUnderwaterEffects m_underwaterEffects;

        private void Start()
        {
            Setup(true);
        }
        private void OnDisable()
        {
            Setup(false);
        }
        private void LateUpdate()
        {
            UpdatePosition();
        }

        /// <summary>
        /// Sets up the underwater fog
        /// </summary>
        public void Setup(bool isEnabled)
        {
            if (m_densityVolume == null)
            {
#if UNITY_2021_2_OR_NEWER
                m_densityVolume = GetComponent<LocalVolumetricFog>();
                if (m_densityVolume == null)
                {
                    m_densityVolume = gameObject.AddComponent<LocalVolumetricFog>();
                }
#else
                	m_densityVolume = GetComponent<DensityVolume>();
                	if (m_densityVolume == null)
                	{
	                    m_densityVolume = gameObject.AddComponent<DensityVolume>();
    	            }
                
#endif
            }

            UnderwaterProfile.Apply(m_densityVolume);
            if (m_underwaterEffects == null)
            {
                m_underwaterEffects = GaiaUtils.FindOOT<GaiaUnderwaterEffects>();
            }

            if (m_gaiaGlobal == null)
            {
                m_gaiaGlobal = GaiaGlobal.Instance;
            }
            if (m_gaiaGlobal != null)
            {
                m_camera = m_gaiaGlobal.m_mainCamera;
            }

            m_densityVolume.enabled = isEnabled;
            if (m_underwaterEffects != null)
            {
                transform.SetParent(m_underwaterEffects.transform);
            }
        }
        /// <summary>
        /// Updates the position
        /// </summary>
        private void UpdatePosition()
        {
            if (m_camera != null)
            {
                transform.position = m_camera.transform.position;
            }
        }
#endif
    }
}