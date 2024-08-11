using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;

public class PulseEmission : MonoBehaviour
{
    [SerializeField] Renderer rend;
    [FormerlySerializedAs("_pulseSpeed")] [SerializeField] float pulseSpeed = 1f;
    [SerializeField, ColorUsage(true, true)] Color fromColor = Color.white;
    [SerializeField, ColorUsage(true, true)] Color toColor = Color.white;
    
    private float _currentEmission;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    // Tween _tween;
    //
    // private void Awake()
    // {
    //     if (rend == null)
    //         rend = GetComponent<Renderer>();
    //
    //     rend.material.SetColor(EmissionColor, fromColor);
    //     
    //     _tween = rend.material.DOColor(toColor, "_EmissionColor", pulseSpeed).SetLoops(-1, LoopType.Yoyo);
    //
    // }

    // private void Update()
    // {
    //     if (rend == null) return;
    //     
    //     _currentEmission = Mathf.PingPong(Time.deltaTime * pulseSpeed, maxEmission - minEmission) + minEmission;
    //     rend.material.SetColor(EmissionColor, Color.Lerp(fromColor, toColor, _currentEmission));
    // }
    //
    //
    

    // private void Update()
    // {
    //     if (rend == null) return;
    //     if (_tween.IsPlaying() == false)
    //         _tween.Play();
    // }
}
