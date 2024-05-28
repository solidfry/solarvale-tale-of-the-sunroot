using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gaia
{
    public class PhotoModeColorPicker : MonoBehaviour
    {
        public Color m_value;
        public Image m_currentColor;
        public Image m_lastColor;
        public Color m_lastColorValue;
        public Slider m_sliderR;
        public Image m_currentRedColor;
        public Slider m_sliderG;
        public Image m_currentGreenColor;
        public Slider m_sliderB;
        public Image m_currentBlueColor;
        public Text m_selectedNameText;
        public GameObject m_hdrSetting;
        public Slider m_sliderHDR;
        public Image m_currentHDRColor;

        public UnityAction m_onChanged;
        private bool m_hdrEnabled = false;

        private void Awake()
        {
            if (m_sliderR != null)
            {
                m_sliderR.onValueChanged.AddListener(SetRedValue);
            }
            if (m_sliderG != null)
            {
                m_sliderG.onValueChanged.AddListener(SetGreenValue);
            }
            if (m_sliderB != null)
            {
                m_sliderB.onValueChanged.AddListener(SetBlueValue);
            }
            if (m_sliderHDR != null)
            {
                m_sliderHDR.onValueChanged.AddListener(SetHDRValue);
            }
        }
        private void Update()
        {
            Refresh();
        }
        public void SetColorValue(Color color, bool hdr)
        {
            m_value = color;
            m_hdrEnabled = hdr;
            if (m_sliderR != null)
            {
                m_sliderR.SetValueWithoutNotify(color.r);
                if (m_currentRedColor != null)
                {
                    Color newColor = m_value;
                    newColor = Color.red;
                    newColor.r *= color.r;
                    if (hdr)
                    {
                        if (color.a != 0f && color.a > 1f || color.a < 0f)
                        {
                            newColor.r *= color.a;
                        }

                        m_value.r = newColor.r;
                    }
                    newColor.a = 1f;
                    m_currentRedColor.color = newColor;
                }
            }
            if (m_sliderG != null)
            {
                m_sliderG.SetValueWithoutNotify(color.g);
                if (m_currentGreenColor != null)
                {
                    Color newColor = m_value;
                    newColor = Color.green;
                    newColor.g *= color.g;
                    if (hdr)
                    {
                        if (color.a != 0f && color.a > 1f || color.a < 0f)
                        {
                            newColor.g *= color.a;
                        }
                        m_value.g = newColor.g;
                    }
                    newColor.a = 1f;
                    m_currentGreenColor.color = newColor;
                }
            }
            if (m_sliderB != null)
            {
                m_sliderB.SetValueWithoutNotify(color.b);
                if (m_currentBlueColor != null)
                {
                    Color newColor = m_value;
                    newColor = Color.blue;
                    newColor.b *= color.b;
                    if (hdr)
                    {
                        if (color.a != 0f && color.a > 1f || color.a < 0f)
                        {
                            newColor.b *= color.a;
                        }
                        m_value.b = newColor.b;
                    }
                    newColor.a = 1f;
                    m_currentBlueColor.color = newColor;
                }
            }
            if (m_hdrSetting != null)
            {
                if (hdr)
                {
                    m_value.a = color.a;
                    m_hdrSetting.SetActive(true);
                    if (m_sliderHDR != null)
                    {
                        m_sliderHDR.SetValueWithoutNotify(color.a);
                        if (m_currentHDRColor != null)
                        {
                            Color newColor = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(-5f, 5f, color.a));
                            m_currentHDRColor.color = newColor;
                        }
                    }
                }
                else
                {
                    m_hdrSetting.SetActive(false);
                }
            }

            Refresh();
            this.transform.SetAsLastSibling();
        }
        public void SetLastColorValue(Color color)
        {
            m_lastColorValue = color;
            if (m_lastColor != null)
            {
                Color newColor = color;
                newColor.a = 1f;
                m_lastColor.color = newColor;
            }
        }
        public void SetCurrentFocusedName(string name)
        {
            if (m_selectedNameText != null)
            {
                m_selectedNameText.text = "(" + name + ")";
            }
        }
        public void CloseColorPicker()
        {
            gameObject.SetActive(false);
            if (PhotoMode.Instance != null)
            {
                PhotoMode.Instance.m_updateColorPickerRef = false;
            }
        }
        public void RefColor(ref Color color, ref Button currentValue, bool applyHDR)
        {
            color = m_value;
            if (currentValue != null)
            {
                Color newColor = m_value;
                if (m_hdrEnabled && applyHDR)
                {
                    if (color.a != 0f && color.a > 1f || color.a < 0f)
                    {
                        newColor *= m_value.a;
                    }
                }
                newColor.a = 1f;
                color = newColor;
                ColorBlock colorBlock = currentValue.colors;
                colorBlock.normalColor = newColor;
                colorBlock.selectedColor = newColor;
                currentValue.colors = colorBlock;
            }
        }
        public void Refresh()
        {
            UpdateCurrentColors();
        }
        private void UpdateCurrentColors()
        {
            if (m_currentColor != null)
            {
                Color newColor = m_value;
                if (m_hdrEnabled)
                {
                    if (m_value.a != 0f && m_value.a > 1f || m_value.a < 0f)
                    {
                        newColor *= m_value.a;
                    }
                }

                newColor.a = 1f;
                m_currentColor.color = newColor;
            }
        }
        public void SetRedValue(float value)
        {
            m_value.r = value;
            if (m_currentRedColor != null)
            {
                Color newColor = m_value;
                newColor = Color.red;
                newColor.r *= value;
                newColor.a = 1f;
                m_currentRedColor.color = newColor;
                if (m_onChanged != null)
                {
                    m_onChanged.Invoke();
                }
            }
        }
        public void SetGreenValue(float value)
        {
            m_value.g = value;
            if (m_currentGreenColor != null)
            {
                Color newColor = m_value;
                newColor = Color.green;
                newColor.g *= value;
                newColor.a = 1f;
                m_currentGreenColor.color = newColor;
                if (m_onChanged != null)
                {
                    m_onChanged.Invoke();
                }
            }
        }
        public void SetBlueValue(float value)
        {
            m_value.b = value;
            if (m_currentBlueColor != null)
            {
                Color newColor = m_value;
                newColor = Color.blue;
                newColor.b *= value;
                newColor.a = 1f;
                m_currentBlueColor.color = newColor;
                if (m_onChanged != null)
                {
                    m_onChanged.Invoke();
                }
            }
        }
        public void SetHDRValue(float value)
        {
            m_value.a = value;
            if (m_currentHDRColor != null)
            {
                Color newColor = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(-5f, 5f, value));
                m_currentHDRColor.color = newColor;
                if (m_onChanged != null)
                {
                    m_onChanged.Invoke();
                }
            }
        }
        public void UpdateOnChangedMethod(UnityAction onChanged)
        {
            m_onChanged = onChanged;
        }
        public void ResetColorValue()
        {
            if (m_lastColor != null)
            {
                if (PhotoMode.Instance != null)
                {
                    m_sliderR.value = m_lastColorValue.r;
                    m_sliderG.value = m_lastColorValue.g;
                    m_sliderB.value = m_lastColorValue.b;
                    m_sliderHDR.value = m_lastColorValue.a;

                    PhotoMode.Instance.UpdateColorPicker();
                    if (m_onChanged != null)
                    {
                        m_onChanged.Invoke();
                    }
                }
            }
        }
    }
}