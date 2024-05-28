using System.Collections.Generic;
#if GAIA_PRO_PRESENT
using ProceduralWorlds.HDRPTOD;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gaia
{
    public enum RuntimeUIVectorMode { Vector2, Vector3 }
    public enum RuntimeUIVectorSetInputMode { VectorX, VectorY, VectorZ }
    public class PhotoModeUIHelper : MonoBehaviour
    {
        [HideInInspector]
        public bool m_isUsingSlider = true;

        public Text[] m_labels;
        public Slider m_slider;
        public InputField[] m_inputs;
        public Toggle m_toggle;
        public Dropdown m_dropdown;
        public Button[] m_buttons;
        public Image m_image;
        //public Image m_colorPreviewImage;
        public Button m_colorPreviewButton;

        public GameObject m_headerObject;
        public GameObject m_labelPanelObject;
        public GameObject m_propertyPanelObject;
        public Text m_headerText;

        //Vector
        public GameObject m_vectorsObject;
        public GameObject m_vector3Object;
        public GameObject m_vector2Object;
        //Vector 3
        public InputField m_vector3InputX;
        public InputField m_vector3InputY;
        public InputField m_vector3InputZ;
        //Vector 2
        public InputField m_vector2InputX;
        public InputField m_vector2InputY;

        private void Awake()
        {
            m_isUsingSlider = true;
        }
        #region Public UI Functions

        #region Images

        public void SetImage(PhotoModeImages imageData)
        {
            if (m_image != null && imageData != null)
            {
                m_image.sprite = imageData.m_image;
                LayoutElement layoutElement = m_image.GetComponent<LayoutElement>();
                if (layoutElement != null)
                {
                    layoutElement.preferredWidth = imageData.m_imageWidthAndHeight.x;
                    layoutElement.preferredHeight = imageData.m_imageWidthAndHeight.y;
                }
            }
        }
        public void SetImageVisability(bool visable)
        {
            if (m_image != null)
            {
                m_image.gameObject.SetActive(visable);
            }
        }

        #endregion
        #region Label

        public string GetLabel(int labelIndex = 0)
        {
            if (labelIndex < 0 || labelIndex >= m_labels.Length || m_labels[labelIndex] == null)
            {
                return "";
            }

            return m_labels[labelIndex].text;
        }
        public void SetLabel(string text, int labelIndex = 0)
        {
            if (labelIndex < 0 || labelIndex >= m_labels.Length || m_labels[labelIndex] == null)
            {
                return;
            }

            m_labels[labelIndex].text = text;
        }
        public void SetLabelColor(Color colour, int labelIndex = 0)
        {
            if (labelIndex < 0 || labelIndex >= m_labels.Length)
            {
                return;
            }

            m_labels[labelIndex].color = colour;
        }
        public void SetLabelVisible(params bool[] labelsVisible)
        {
            for (int i = 0; i < m_labels.Length; ++i)
            {
                if (i >= labelsVisible.Length)
                {
                    if (m_labels[i] != null)
                    {
                        m_labels[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (m_labels[i] != null)
                    {
                        m_labels[i].gameObject.SetActive(labelsVisible[i]);
                    }
                }
            }
        }

        #endregion
        #region Input

        public string GetInput(int inputIndex = 0)
        {
            if (inputIndex < 0 || inputIndex >= m_inputs.Length || m_inputs[inputIndex] == null)
            {
                return "";
            }

            return m_inputs[inputIndex].text;
        }
        public void SetInput(string text, int inputIndex = 0)
        {
            if (inputIndex < 0 || inputIndex >= m_inputs.Length || m_inputs[inputIndex] == null)
            {
                return;
            }

            m_inputs[inputIndex].text = text;
        }
        public void SetInput(string text, UnityAction<string> onChanged, int inputIndex = 0, InputField.CharacterValidation characterValidation = InputField.CharacterValidation.Decimal, InputField.ContentType contentType = InputField.ContentType.DecimalNumber)
        {
            if (inputIndex < 0 || inputIndex >= m_inputs.Length || m_inputs[inputIndex] == null)
            {
                return;
            }

            m_inputs[inputIndex].text = text;
            m_inputs[inputIndex].characterValidation = characterValidation;
            m_inputs[inputIndex].contentType = contentType;
            m_inputs[inputIndex].onValueChanged.AddListener(onChanged);
        }
        public void SetInputVisible(params bool[] inputsVisible)
        {
            for (int i = 0; i < m_inputs.Length; ++i)
            {
                if (i >= inputsVisible.Length)
                {
                    if (m_inputs[i] != null)
                    {
                        m_inputs[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (m_inputs[i] != null)
                    {
                        m_inputs[i].gameObject.SetActive(inputsVisible[i]);
                    }
                }
            }
        }

        #endregion
        #region Button

        public void SetButtonLabel(string text, int buttonIndex = 0)
        {
            if (buttonIndex < 0 || buttonIndex >= m_buttons.Length || m_buttons[buttonIndex] == null)
            {
                return;
            }
            Text txt = m_buttons[buttonIndex].GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = text;
            }
        }
        public void SetButton(string text, UnityAction onClick, int buttonIndex = 0)
        {
            if (buttonIndex < 0 || buttonIndex >= m_buttons.Length || m_buttons[buttonIndex] == null)
            {
                return;
            }
            m_buttons[buttonIndex].onClick.AddListener(onClick);
            Text txt = m_buttons[buttonIndex].GetComponentInChildren<Text>();
            if (txt != null)
            {
                txt.text = text;
            }
        }
        public void SetButtonVisible(params bool[] buttonVisible)
        {
            for (int i = 0; i < m_buttons.Length; ++i)
            {
                if (i >= buttonVisible.Length)
                {
                    if (m_buttons[i] != null)
                    {
                        m_buttons[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (m_buttons[i] != null)
                    {
                        m_buttons[i].gameObject.SetActive(buttonVisible[i]);
                    }
                }
            }
        }
        public void SetButtonInactive(bool inactive)
        {
            for (int i = 0; i < m_buttons.Length; i++)
            {
                m_buttons[i].interactable = inactive;
            }
        }

        #endregion
        #region Toggle

        public bool GetToggle()
        {
            if (m_toggle == null)
            {
                return false;
            }

            return m_toggle.isOn;
        }
        public void SetToggle(bool on)
        {
            if (m_toggle == null)
            {
                return;
            }

            m_toggle.isOn = on;
        }
        public void SetToggle(bool on, UnityAction<bool> onChange)
        {
            if (m_toggle == null)
            {
                return;
            }

            m_toggle.isOn = on;
            m_toggle.onValueChanged.AddListener(onChange);
        }
        public void SetToggleVisible(bool visible)
        {
            if (m_toggle == null)
            {
                return;
            }

            m_toggle.gameObject.SetActive(visible);
        }

        #endregion
        #region Slider

        public float GetSlider()
        {
            if (m_slider != null)
            {
                return m_slider.value;
            }

            return 0f;
        }
        public void SetSlider(float value)
        {
            if (m_slider != null)
            {
                m_slider.value = value;
            }
        }
        public void SetSliderMinMax(float min, float max)
        {
            if (m_slider != null)
            {
                m_slider.minValue = min;
                m_slider.maxValue = max;
            }
        }
        public void SetSlider(float value, UnityAction<float> onChanged)
        {
            if (m_slider != null)
            {
                m_slider.value = value;
                m_slider.onValueChanged.AddListener(onChanged);
            }
        }
        public void SetSlider(float value, float min, float max, UnityAction<float> onChanged)
        {
            if (m_slider != null)
            {
                m_slider.value = value;
                m_slider.wholeNumbers = false;
                m_slider.minValue = min;
                m_slider.maxValue = max;
                m_slider.onValueChanged.AddListener(onChanged);
            }
        }
        public void SetSlider(int value, int min, int max, UnityAction<float> onChanged)
        {
            if (m_slider != null)
            {
                m_slider.value = value;
                m_slider.wholeNumbers = true;
                m_slider.minValue = min;
                m_slider.maxValue = max;
                m_slider.onValueChanged.AddListener(onChanged);
            }
        }
        public void SetSliderVisible(bool visible)
        {
            if (m_slider == null)
            {
                return;
            }
            m_slider.gameObject.SetActive(visible);
        }
        public void SetValue(float value, bool setInput = true)
        {
            SetSlider(value);
            if (setInput)
            {
                SetInput(value.ToString());
            }
        }
        public void SetValue(float value, string formatString)
        {
            SetSlider(value);
            SetInput(value.ToString(formatString));
        }

        #endregion
        #region Dropdown

        public void SetDropdownOptions(List<Sprite> options)
        {
            if (m_dropdown != null)
            {
                m_dropdown.AddOptions(options);
                m_dropdown.RefreshShownValue();
            }
        }
        public void SetDropdownOptions(List<string> options)
        {
            if (m_dropdown != null)
            {
                m_dropdown.ClearOptions();
                m_dropdown.AddOptions(options);
                m_dropdown.RefreshShownValue();
            }
        }
        public void SetDropdown(int value, UnityAction<int> onChange)
        {
            if (m_dropdown == null)
            {
                return;
            }

            m_dropdown.onValueChanged.AddListener(onChange);
            m_dropdown.value = value;
        }
        public void SetDropdownValue(int value)
        {
            if (m_dropdown != null)
            {
                m_dropdown.value = value;
            }
        }
        public void SetDropdownVisability(bool visable)
        {
            if (m_dropdown == null)
            {
                return;
            }

            m_dropdown.gameObject.SetActive(visable);
        }

        #endregion
        #region Header

        public void SetHeaderVisible(bool visable)
        {
            if (m_headerObject != null)
            {
                m_headerObject.SetActive(visable);
                if (visable)
                {
                    if (m_labelPanelObject != null)
                    {
                        m_labelPanelObject.SetActive(false);
                    }
                    if (m_propertyPanelObject != null)
                    {
                        m_propertyPanelObject.SetActive(false);
                    }
                }
                else
                {
                    if (m_labelPanelObject != null)
                    {
                        m_labelPanelObject.SetActive(true);
                    }
                    if (m_propertyPanelObject != null)
                    {
                        m_propertyPanelObject.SetActive(true);
                    }
                }
            }
        }
        public void SetHeaderText(string text)
        {
            if (m_headerText != null)
            {
                m_headerText.text = text;
            }
        }

        #endregion
        #region Color Picker

        public void SetColorPreviewImage(Color color, bool hdr = false)
        {
            if (m_colorPreviewButton != null)
            {
                Color newColor = color;
                if (hdr)
                {
                    if (color.a != 0f && color.a > 1f || color.a < 0f)
                    {
                        newColor *= color.a;
                    }
                }
                newColor.a = 1f;
                ColorBlock colorBlock = m_colorPreviewButton.colors;
                colorBlock.selectedColor = newColor;
                colorBlock.normalColor = newColor;
                m_colorPreviewButton.colors = colorBlock;
            }
        }

        public void SetColorPreviewOnClicked(UnityAction onClicked)
        {
            if (m_colorPreviewButton != null)
            {
                m_colorPreviewButton.onClick.AddListener(onClicked);
            }
        }
        public void SetColorPreviewImageVisable(bool visable)
        {
            if (m_colorPreviewButton != null)
            {
                m_colorPreviewButton.gameObject.SetActive(visable);
            }
        }

        #endregion
        #region Vector

        public void SetVectorVisable(bool visable, RuntimeUIVectorMode vectorMode = RuntimeUIVectorMode.Vector3)
        {
            if (m_vectorsObject != null && m_vector3Object != null && m_vector2Object != null)
            {
                m_vectorsObject.SetActive(visable);

                switch (vectorMode)
                {
                    case RuntimeUIVectorMode.Vector2:
                    {
                        m_vector2Object.SetActive(visable);
                        m_vector3Object.SetActive(false);
                        break;
                    }
                    case RuntimeUIVectorMode.Vector3:
                    {
                        m_vector3Object.SetActive(visable);
                        m_vector2Object.SetActive(false);
                        break;
                    }
                }
            }
        }
        public void SetVector(Vector3 value, UnityAction<string> onChangedX, UnityAction<string> onChangedY, UnityAction<string> onChangedZ)
        {
            if (m_vector3InputX != null && m_vector3InputY != null && m_vector3InputZ != null)
            {
                m_vector3InputX.text = value.x.ToString(PhotoModeUtils.m_floatFormat);
                m_vector3InputY.text = value.y.ToString(PhotoModeUtils.m_floatFormat);
                m_vector3InputZ.text = value.z.ToString(PhotoModeUtils.m_floatFormat);
                m_vector3InputX.onValueChanged.AddListener(onChangedX);
                m_vector3InputY.onValueChanged.AddListener(onChangedY);
                m_vector3InputZ.onValueChanged.AddListener(onChangedZ);
            }
        }
        public void SetVector(Vector2 value, UnityAction<string> onChangedX, UnityAction<string> onChangedY)
        {
            if (m_vector2InputX != null && m_vector2InputY != null)
            {
                m_vector2InputX.text = value.x.ToString(PhotoModeUtils.m_floatFormat);
                m_vector2InputY.text = value.y.ToString(PhotoModeUtils.m_floatFormat);
                m_vector2InputX.onValueChanged.AddListener(onChangedX);
                m_vector2InputY.onValueChanged.AddListener(onChangedY);
            }
        }
        public void SetVectorValue(float value, RuntimeUIVectorMode vectorMode, RuntimeUIVectorSetInputMode vectorInputMode)
        {
            switch (vectorMode)
            {
                case RuntimeUIVectorMode.Vector3:
                {
                    switch (vectorInputMode)
                    {
                        case RuntimeUIVectorSetInputMode.VectorX:
                        {
                            if (m_vector3InputX != null)
                            {
                                m_vector3InputX.text = value.ToString();
                            }
                            break;
                        }
                        case RuntimeUIVectorSetInputMode.VectorY:
                        {
                            if (m_vector3InputY != null)
                            {
                                m_vector3InputY.text = value.ToString();
                            }
                            break;
                        }
                        case RuntimeUIVectorSetInputMode.VectorZ:
                        {
                            if (m_vector3InputZ != null)
                            {
                                m_vector3InputZ.text = value.ToString();
                            }
                            break;
                        }
                    }
                    break;
                }
                case RuntimeUIVectorMode.Vector2:
                {
                    switch (vectorInputMode)
                    {
                        case RuntimeUIVectorSetInputMode.VectorX:
                        {
                            if (m_vector2InputX != null)
                            {
                                m_vector2InputX.text = value.ToString();
                            }
                            break;
                        }
                        case RuntimeUIVectorSetInputMode.VectorY:
                        {
                            if (m_vector2InputY != null)
                            {
                                m_vector2InputY.text = value.ToString();
                            }
                            break;
                        }
                    }
                    break;
                }
            }
        }

        #endregion

        #endregion
        #region Utils

        /// <summary>
        /// On cahnged event used to set the slider being used value to true to help with input setup for decials on the "Input Field'
        /// </summary>
        /// <param name="value"></param>
        public void SetIsUsingSlider()
        {
            m_isUsingSlider = true;
        }
        /// <summary>
        /// Used to sync the label with the hdrp time of day
        /// </summary>
        public void SyncHDRPTimeOfDay()
        {
#if HDPipeline && UNITY_2021_2_OR_NEWER && GAIA_PRO_PRESENT
            if (HDRPTimeOfDayAPI.GetTimeOfDay())
            {
                SetInput(HDRPTimeOfDayAPI.GetCurrentTime().ToString());
            }
#endif
        }

        #endregion
    }
}