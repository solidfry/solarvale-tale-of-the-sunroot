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
        
        [SerializeField] Color morningFogTintColor = Color.white;
        [SerializeField] Color dayFogTintColor = Color.white;
        [SerializeField] Color eveningFogTintColor = Color.white;
        [SerializeField] Color nightFogTintColor = Color.white;
        
        [SerializeField] Volume volume;
    
        [SerializeField] float fogHDRIntensity = 1.0f;
    
        ButoVolumetricFog _fogVolume;
    
        UniStormSystem _uniStormSystem;
    
        ColorParameter _fogColorParam = new ColorParameter(Color.white);
    
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
                    UpdateFog(_uniStormSystem.m_SunLight.color * morningFogTintColor);
                    break;
                case UniStormSystem.CurrentTimeOfDayEnum.Day:
                    UpdateFog( _uniStormSystem.m_SunLight.color * dayFogTintColor);
                    break;
                case UniStormSystem.CurrentTimeOfDayEnum.Evening:
                    UpdateFog(_uniStormSystem.m_MoonLight.color * eveningFogTintColor);
                    break;
                case UniStormSystem.CurrentTimeOfDayEnum.Night:
                    UpdateFog(_uniStormSystem.m_MoonLight.color * nightFogTintColor);
                    break;
            }
        }

        void UpdateFog(Color l)
        {
            fogColor = l;
            _fogColorParam.value = fogColor * fogHDRIntensity;
            _fogVolume.directionalForward.value = _fogColorParam.value;
            _fogVolume.litColor.value = _fogColorParam.value;
        }
    
    }
}
