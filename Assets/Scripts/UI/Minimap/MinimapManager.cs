using UnityEngine;

namespace UI.Minimap
{
    public class MinimapManager : MonoBehaviour
    {
        [SerializeField] private MinimapController miniMap;

        private void Initialise(MinimapController mapObject)
        {
            if (mapObject is null) return;
            miniMap = mapObject;
            miniMap.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (miniMap is null)
            {
                miniMap = FindObjectOfType<MinimapController>();
                Initialise(miniMap);
            }
        }

        public void UnlockMiniMap()
        {
            if (miniMap is null) return;
            miniMap.gameObject.SetActive(true);
            miniMap.Initialise();
        }
    }
}
