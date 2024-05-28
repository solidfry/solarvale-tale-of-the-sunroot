using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public class ColorPreviewSync : MonoBehaviour
    {
        public Image m_copyColorFrom;
        public Button m_copyColorTo;
        public bool m_canSync = false;
        public float m_highlightedColorMultiplier = 1.5f;

        private void Update()
        {
            if (!m_canSync)
            {
                return;
            }

            if (m_copyColorFrom != null && m_copyColorTo != null)
            {
                ColorBlock colorBlock = m_copyColorTo.colors;
                if (m_copyColorFrom.color == Color.black)
                {
                    colorBlock.highlightedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                }
                else
                {
                    colorBlock.highlightedColor = m_copyColorFrom.color * 2.5f;
                }

                m_copyColorTo.colors = colorBlock;
            }
        }
    }
}