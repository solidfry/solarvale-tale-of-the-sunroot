using TimeCycle.ScriptableObjects;
using TimeCycle.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace TimeCycle
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private TimeSettings timeSettings;
        [SerializeField] private TimeUI timeUI;
        
        [Header("Sun and Moon")]
        [SerializeField] private Light sun;
        [SerializeField] private Light moon;
        [SerializeField] AnimationCurve lightIntensityCurve;
        [SerializeField] float maxSunIntensity = 1;
        [SerializeField] float maxMoonIntensity = 0.5f;
        [SerializeField] Color dayAmbientLight;
        [SerializeField] Color nightAmbientLight;
        [SerializeField] private Volume volume;
        
        ColorAdjustments colorAdjustments;
        
        TimeService _timeService;
        
        private void Awake()
        {
            _timeService = new TimeService(timeSettings);
        }

        private void Start()
        {
            volume.profile.TryGet(out colorAdjustments);
        }

        private void Update()
        {
            UpdateTimeOfDay();
            // RotateMoon();
            RotateSun();
            UpdateLightSettings();
        }

        private void UpdateTimeOfDay()
        {
            _timeService.UpdateTime(Time.deltaTime);
            
            if (timeUI != null)
                timeUI.UpdateText(_timeService.CurrentTime);
        }
        
        void RotateSun()
        {
            if (sun == null)
                return;
            float rot = _timeService.CalculateSunAngle();
            sun.transform.rotation = Quaternion.AngleAxis(rot, Vector3.right);
        }
        
        void RotateMoon()
        {
            if (moon == null)
                return;
            float rot = _timeService.CalculateSunAngle() + 180;
            moon.transform.rotation = Quaternion.AngleAxis(rot, Vector3.right);
        }

        void UpdateLightSettings()
        {
            float dotProduct = Vector3.Dot(sun.transform.forward, Vector3.down);
            sun.intensity = Mathf.Lerp( 0, maxSunIntensity, lightIntensityCurve.Evaluate(dotProduct) );
            moon.intensity = Mathf.Lerp( maxMoonIntensity, 0, lightIntensityCurve.Evaluate(dotProduct) );
            
            if (colorAdjustments == null) return;

            colorAdjustments.colorFilter.value = Color.Lerp(nightAmbientLight, dayAmbientLight, lightIntensityCurve.Evaluate(dotProduct));
            
        }
    }
}