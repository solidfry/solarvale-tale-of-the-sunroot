using OccaSoftware.Buto.Runtime;
using UniStorm;
using UnityEngine;
using UnityEngine.Rendering;

namespace Lighting
{
    public class FogLightSynchroniser : MonoBehaviour
    {
        RenderSettings _renderSettings;
        [SerializeField, ColorUsage(false, true)] Color fogColor = Color.white;
        
        [Header("Fog Lit Settings")]
        [SerializeField] Color morningFogTintColor = Color.white;
        [SerializeField] Color dayFogTintColor = Color.white;
        [SerializeField] Color eveningFogTintColor = Color.white;
        [SerializeField] Color nightFogTintColor = Color.white;
        [SerializeField] float morningFogHDRIntensity = 1.0f, dayFogHDRIntensity = 1.0f, eveningFogHDRIntensity = 1.0f, nightFogHDRIntensity = 1.0f;    
        
        [Header("Fog Shadow Settings")]
        [SerializeField] Color morningShadowTintColor = Color.white;
        [SerializeField] Color dayShadowTintColor = Color.white;
        [SerializeField] Color eveningShadowTintColor = Color.white;
        [SerializeField] Color nightShadowTintColor = Color.white;
        [SerializeField] float morningShadowHDRIntensity = 1.0f, dayShadowHDRIntensity = 1.0f, eveningShadowHDRIntensity = 1.0f, nightShadowHDRIntensity = 1.0f;
        
        [SerializeField] Volume volume;
    
        ButoVolumetricFog _fogVolume;
    
        UniStormSystem _uniStormSystem;
    
        ColorParameter _fogColorParam = new ColorParameter(Color.white);
        ColorParameter _fogShadowColorParam = new ColorParameter(Color.white);
    
        [SerializeField] UniStormSystem.CurrentTimeOfDayEnum timeOfDay = UniStormSystem.CurrentTimeOfDayEnum.Day;
        
        private void Awake()
        {
            volume = GetComponent<Volume>();
            volume.profile.TryGet(out _fogVolume);
        }
            

        private void Start()
        {
            GetUnistormSystem();
        }

        private void GetUnistormSystem()
        {
            if (_uniStormSystem is not null) return;
        
            _uniStormSystem = UniStormSystem.Instance;
        }
        
        private void Update()
        {
            if (_fogVolume is null) return;
        
            GetUnistormSystem();
            
            ApplyFogSettingsToVolume();
        }

        private void ApplyFogSettingsToVolume()
        {
            if (_uniStormSystem is null) return;
            
            timeOfDay = _uniStormSystem.CurrentTimeOfDay;
            
            switch (timeOfDay)
            {
                case UniStormSystem.CurrentTimeOfDayEnum.Morning:
                    UpdateLitFog(_uniStormSystem.m_SunLight.color * morningFogTintColor, morningFogHDRIntensity);
                    UpdateShadowFog( _uniStormSystem.m_SunLight.color * morningShadowTintColor, morningShadowHDRIntensity);
                    break;
                case UniStormSystem.CurrentTimeOfDayEnum.Day:
                    UpdateLitFog( _uniStormSystem.m_SunLight.color * dayFogTintColor, dayFogHDRIntensity);
                    UpdateShadowFog( _uniStormSystem.m_SunLight.color * dayShadowTintColor, dayShadowHDRIntensity);
                    break;
                case UniStormSystem.CurrentTimeOfDayEnum.Evening:
                    UpdateLitFog(_uniStormSystem.m_MoonLight.color * eveningFogTintColor, eveningFogHDRIntensity);
                    UpdateShadowFog(_uniStormSystem.m_MoonLight.color * eveningShadowTintColor, eveningShadowHDRIntensity);
                    break;
                case UniStormSystem.CurrentTimeOfDayEnum.Night:
                    UpdateLitFog(_uniStormSystem.m_MoonLight.color * nightFogTintColor, nightFogHDRIntensity);
                    UpdateShadowFog(_uniStormSystem.m_MoonLight.color * nightShadowTintColor, nightShadowHDRIntensity);
                    break;
            }
        }

        void UpdateLitFog(Color l, float intensity = 1.0f)
        {
            fogColor = l;
            _fogColorParam.value = fogColor * intensity;
            _fogVolume.directionalForward.value = _fogColorParam.value;
            _fogVolume.litColor.value = _fogColorParam.value;
        }
        
        void UpdateShadowFog(Color l, float intensity = 1.0f)
        {
            fogColor = l;
            _fogShadowColorParam.value = fogColor * intensity;
            _fogVolume.directionalBack.value = _fogShadowColorParam.value;
            _fogVolume.shadowedColor.value = _fogShadowColorParam.value;
        }
    
    }
}
