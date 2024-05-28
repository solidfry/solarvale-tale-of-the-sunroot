using UnityEngine;
using UnityEngine.EventSystems;

namespace Gaia
{
    public class DraggableUIWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        public RectTransform m_draggableRect;
        public Canvas m_canvas;

        private Vector2 m_resetPosition;

        private void Awake()
        {
            if (m_draggableRect != null)
            {
                m_resetPosition = m_draggableRect.anchoredPosition;
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (m_draggableRect != null && m_canvas != null)
            {
                Vector2 position = m_draggableRect.anchoredPosition += eventData.delta / m_canvas.scaleFactor;
                position.x = -Mathf.Clamp(Mathf.Abs(position.x), m_draggableRect.rect.width / 2f, Screen.currentResolution.width - (m_draggableRect.rect.width / 2f));
                position.y = Mathf.Clamp(Mathf.Abs(position.y), m_draggableRect.rect.height / 2f, Screen.currentResolution.height - (m_draggableRect.rect.height / 2f));
                m_draggableRect.anchoredPosition = position;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (m_draggableRect != null)
            {
                m_draggableRect.SetAsLastSibling();

                if (Input.GetMouseButtonDown(2))
                {
                    m_draggableRect.anchoredPosition = m_resetPosition;
                }
            }
        }
    }
}