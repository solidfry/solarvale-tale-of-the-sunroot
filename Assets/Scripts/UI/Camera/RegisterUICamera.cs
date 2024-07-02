using Events;
using UnityEngine;

namespace UI.Camera
{
    [RequireComponent(typeof(Canvas))]
    public class RegisterUICamera : MonoBehaviour
    {
        Canvas _canvas;
        [SerializeField] bool registerOnStart = true;

        private void Awake() => _canvas = GetComponent<Canvas>();

        private void Start()
        {
            if (!registerOnStart) return;
            RegisterWithCamera();
        }

        private void RegisterWithCamera() => GlobalEvents.OnRegisterUIWithCameraEvent?.Invoke(_canvas);
    }
}