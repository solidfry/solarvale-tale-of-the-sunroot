using System.Collections.Generic;
using Events;
using UnityEngine;

namespace UI.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class UICameraManager : MonoBehaviour
    {
        [SerializeField] private UnityEngine.Camera uiCamera;
    
        [SerializeField] List<Canvas> uiCanvases;
    
        private void Awake()
        {
            uiCamera = GetComponent<UnityEngine.Camera>();
            GlobalEvents.OnRegisterUIWithCameraEvent += RegisterUI;
        }
    
        private void OnDisable()
        {
            GlobalEvents.OnRegisterUIWithCameraEvent -= RegisterUI;
            uiCanvases.Clear();
        }

        private void RegisterUI(Canvas canvas)
        {
            if (uiCanvases.Contains(canvas)) return;
            
            SetCameraScreenSpaceOverlay(canvas);
            
            uiCanvases.Add(canvas);
            canvas.worldCamera = uiCamera;
            canvas.sortingLayerName = "UI";
        }

        private static void SetCameraScreenSpaceOverlay(Canvas canvas)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay) return;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
    }
}
