using System;
using Cinemachine;
using UnityEngine;

namespace CameraSystem
{
    [Serializable]
    public class CameraType
    {
        [field: SerializeField] public GameObject GameObject { get; private set; }
        [field:SerializeField] public CinemachineVirtualCamera Camera { get; private set; }
        [field:SerializeField] public CameraMode Mode { get; private set; }
        
        public int GetPriority => Camera.Priority;
        
        public void SetPriority(int priority)
        {
            Camera.Priority = priority;
        }
        
        public void DisableGameObject()
        {
            GameObject.SetActive(false);
        }
        
    }
}