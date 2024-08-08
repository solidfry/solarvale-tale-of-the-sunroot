using UnityEngine;

namespace Minimap
{
    public class MinimapWorldObject : MonoBehaviour
    {
        [SerializeField] private bool followObject = false;
        [SerializeField] private Sprite minimapIcon;
        public Sprite MinimapIcon => minimapIcon;

        private void OnEnable()
        {
            MinimapController.Instance?.RegisterMinimapWorldObject(this, followObject);
        }

        private void OnDisable()
        {
            MinimapController.Instance?.RemoveMinimapWorldObject(this);
        }
    }
}
