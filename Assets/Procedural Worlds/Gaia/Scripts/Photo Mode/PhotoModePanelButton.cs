using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    public class PhotoModePanelButton : MonoBehaviour
    {
        public bool m_isSet = false;
        public bool m_allowTooltip = true;
        public Button m_button;
        public GridLayoutGroup m_panelLayout;
        public string m_searchPanelName = "Default";
        public Color m_nonSelectedColor = Color.gray;
        public Color m_selectedColor = Color.yellow;
        [HideInInspector]
        public PhotoMode m_photoMode;

        private void Start()
        {
            Register();
        }

        public void Register()
        {
            if (m_photoMode == null)
            {
                m_photoMode = GaiaUtils.FindOOT<PhotoMode>();
            }

            if (m_button == null)
            {
                m_button = GetComponent<Button>();
            }

            if (m_button != null)
            {
                ColorBlock colorBlock = m_button.colors;
                if (m_isSet)
                {
                    colorBlock.normalColor = m_selectedColor;
                    colorBlock.selectedColor = m_selectedColor;
                }
                else
                {
                    colorBlock.normalColor = m_nonSelectedColor;
                    colorBlock.selectedColor = m_nonSelectedColor;
                }

                colorBlock.highlightedColor = m_selectedColor;

                m_button.colors = colorBlock;

                SetButtonState(m_isSet);
                if (m_isSet)
                {
                    m_button.onClick.Invoke();
                }
            }
        }

        public void SetButtonState(bool enabled)
        {
            m_isSet = enabled;
            if (m_button != null)
            {
                ColorBlock colorBlock = m_button.colors;
                if (m_isSet)
                {
                    colorBlock.normalColor = m_selectedColor;
                    colorBlock.selectedColor = m_selectedColor;
                }
                else
                {
                    colorBlock.normalColor = m_nonSelectedColor;
                    colorBlock.selectedColor = m_nonSelectedColor;
                }

                m_button.colors = colorBlock;

                if (m_photoMode != null)
                {
                    if (m_photoMode.m_selectedPanelText != null)
                    {
                        if (m_isSet)
                        {
                            PhotoModePanel profile = m_photoMode.GetPanelProfile(m_searchPanelName);
                            if (profile != null)
                            {
                                m_photoMode.m_selectedPanelText.text = profile.m_shownTitle;
                            }
                        }
                    }
                }
                if (m_panelLayout != null)
                {
                    m_panelLayout.enabled = enabled;
                    m_panelLayout.CalculateLayoutInputVertical();
                    m_panelLayout.SetLayoutVertical();
                    m_panelLayout.CalculateLayoutInputHorizontal();
                    m_panelLayout.SetLayoutHorizontal();
                }
            }
        }
    }
}