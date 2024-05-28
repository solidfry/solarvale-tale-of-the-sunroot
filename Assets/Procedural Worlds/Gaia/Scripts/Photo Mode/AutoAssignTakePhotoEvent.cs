using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public class AutoAssignTakePhotoEvent : MonoBehaviour
    {
        [HideInInspector]
        public ScreenShotter m_screenShotter;
        public Button m_buttonUI;

        private void Start()
        {
            if (m_screenShotter == null)
            {
                m_screenShotter = GaiaUtils.FindOOT<ScreenShotter>();
            }

            if (m_screenShotter != null && m_buttonUI != null)
            {
                m_buttonUI.onClick.AddListener(m_screenShotter.TakeHiResShot);
            }
        }
    }
}