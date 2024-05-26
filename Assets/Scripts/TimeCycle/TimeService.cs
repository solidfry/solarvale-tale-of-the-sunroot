using System;
using TimeCycle.ScriptableObjects;
using UnityEngine;

namespace TimeCycle
{
    public class TimeService 
    {
        readonly TimeSettings _settings;
        DateTime _currentTime;
        readonly TimeSpan _sunriseTime;
        readonly TimeSpan _sunsetTime;

        public event Action OnSunrise;
        public event Action OnSunset;
        public event Action OnHourChange;
        
        private readonly Observable<bool> isDayTime;
        private readonly Observable<int> currentHour;
        
        public TimeService(TimeSettings settings)
        {
            this._settings = settings;
            _currentTime = DateTime.Now.Date + TimeSpan.FromHours(settings.startHour);
            _sunriseTime = TimeSpan.FromHours(settings.sunriseHour);
            _sunsetTime = TimeSpan.FromHours(settings.sunsetHour);
            
            isDayTime = new Observable<bool>(IsDayTime());
            currentHour = new Observable<int>(_currentTime.Hour);
            
            isDayTime.ValueChanged += day => (day ? OnSunrise : OnSunset)?.Invoke();
            currentHour.ValueChanged += _ => OnHourChange?.Invoke();
        }
        
        public void UpdateTime(float deltaTime)
        {
            _currentTime = _currentTime.AddSeconds(deltaTime * _settings.timeMultiplier);
            isDayTime.Value = IsDayTime();
            currentHour.Value = _currentTime.Hour;
        }
        
        public float CalculateSunAngle()
        {
            bool isDay = IsDayTime();
            float startDegree = isDay ? 0 : 180;
            TimeSpan start = isDay ? _sunriseTime : _sunsetTime;
            TimeSpan end = isDay ? _sunsetTime : _sunriseTime;
            TimeSpan totalTime = CalculateDifference(start, end);
            TimeSpan elapsedTime = CalculateDifference(start, _currentTime.TimeOfDay);
            
            double percentage = elapsedTime.TotalMinutes / totalTime.TotalMinutes;
            return Mathf.Lerp( startDegree, startDegree + 180, (float)percentage );
        }
        
        public DateTime CurrentTime => _currentTime;
        
        bool IsDayTime() => _currentTime.TimeOfDay > _sunriseTime && _currentTime.TimeOfDay < _sunsetTime;
        
        TimeSpan CalculateDifference( TimeSpan from, TimeSpan to )
        {
            TimeSpan difference = to - from;
            return difference.TotalHours < 0 ? difference + TimeSpan.FromHours(24) : difference;
        }
        
        
    }
}
