using System;
using Events;
using UnityEngine;

namespace UI.Minimap
{
    public class MinimapManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerCharacter;
        //[SerializeField] private Transform mainCamera;

        [SerializeField] private MiniMapController miniMap;
        
        
        bool _miniMapUnlocked = false;
        
        private void Initialise(MiniMapController mapObject)
        {
            if (mapObject is null) return;
            miniMap = mapObject;
            miniMap.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (miniMap is null)
            {
                miniMap = FindObjectOfType<MiniMapController>();
                Initialise(miniMap);
            }
            
            if (!_miniMapUnlocked) return;
            transform.position = new Vector3(playerCharacter.transform.position.x, 400, playerCharacter.transform.position.z);

            //Vector3 rotation = new Vector3(90, mainCamera.eulerAngles.y, 0);
            //transform.rotation = Quaternion.Euler(rotation);
        }

        public void UnlockMiniMap()
        {
            if (miniMap is null) return;
            miniMap.gameObject.SetActive(true);
            _miniMapUnlocked = true;
            miniMap.Initialise();
        }

    }
}
