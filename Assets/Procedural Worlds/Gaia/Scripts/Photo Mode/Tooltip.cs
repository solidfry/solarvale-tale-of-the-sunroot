using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public class Tooltip : MonoBehaviour
    {
        public Text m_headerText;
        public Text m_contentText;
        public LayoutElement m_layoutElement;
        public int m_contentWrapLimit;

        public RectTransform m_tooltipRect;

        private void Awake()
        {
            TooltipManager.HideTooltip();
            m_tooltipRect = GetComponent<RectTransform>();
        }

        public void SetTooltipText(string content, string header = "")
        {
            m_headerText.text = header;
            m_headerText.gameObject.SetActive(!string.IsNullOrEmpty(header));

            m_contentText.text = content;

            if (m_layoutElement == null)
            {
                return;
            }

            int headerLength = m_headerText.text.Length;
            int contentLength = m_contentText.text.Length;
            m_layoutElement.enabled = (headerLength > m_contentWrapLimit || contentLength > m_contentWrapLimit) ? true : false;
        }
    }
}