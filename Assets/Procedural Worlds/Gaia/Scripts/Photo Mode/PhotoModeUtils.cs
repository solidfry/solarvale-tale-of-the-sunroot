using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gaia
{
    public class PhotoModeUtils
    {
        public static bool m_isSettingValues;
        public static PhotoModeUIHelper m_runtimeUIPrefab = null;
        public static string m_floatFormat = "N1";
        public static string m_floatFormatDecimals = "N4";
        public const int m_metricsWrapLength = 68;

        #region UI Creation Utils

        #region Public

        /// <summary>
        /// Creates a field
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="OnInputChanged"></param>
        public static void CreateField(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, string value, UnityAction<string> OnInputChanged, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInput(value, OnInputChanged);
            runtimeUI.SetInputVisible(true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            OnInputChanged.Invoke(value);
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates slider
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="OnSliderChanged"></param>
        /// <param name="OnInputChanged"></param>
        public static void CreateSlider(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, float value, float min, float max, UnityAction<float> OnSliderChanged, UnityAction<string> OnInputChanged, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetSlider(value, min, max, OnSliderChanged);
            runtimeUI.SetSliderVisible(true);
            runtimeUI.SetInput(value.ToString(m_floatFormat), OnInputChanged);
            runtimeUI.SetInputVisible(true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            OnSliderChanged.Invoke(value);
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates int slider
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="OnSliderChanged"></param>
        /// <param name="OnInputChanged"></param>
        public static void CreateIntSlider(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, int value, int min, int max, UnityAction<float> OnSliderChanged, UnityAction<string> OnInputChanged, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetSlider(value, min, max, OnSliderChanged);
            runtimeUI.SetSliderVisible(true);
            runtimeUI.SetInput(value.ToString(), OnInputChanged, 0, InputField.CharacterValidation.Integer, InputField.ContentType.IntegerNumber);
            runtimeUI.SetInputVisible(true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            OnSliderChanged.Invoke(value);
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates slider
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="display"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="OnSliderChanged"></param>
        public static void CreateSlider(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, float value, string display, float min, float max, UnityAction<float> OnSliderChanged, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabel(display, 1);
            runtimeUI.SetLabelVisible(true, true);
            runtimeUI.SetSlider(value, min, max, OnSliderChanged);
            runtimeUI.SetSliderVisible(true);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            OnSliderChanged.Invoke(value);
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates int slider
        /// </summary>
        /// <param name="slider"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="display"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="OnSliderChanged"></param>
        public static void CreateIntSlider(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, int value, string display, int min, int max, UnityAction<float> OnSliderChanged, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabel(display, 1);
            runtimeUI.SetLabelVisible(true, true);
            runtimeUI.SetSlider(value, min, max, OnSliderChanged);
            runtimeUI.SetSliderVisible(true);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            OnSliderChanged.Invoke(value);
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates toggle
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="OnValueChanged"></param>
        public static void CreateToggle(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, bool value, UnityAction<bool> OnValueChanged, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetToggle(value, OnValueChanged);
            runtimeUI.SetToggleVisible(true);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            OnValueChanged.Invoke(value);
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates dropdown
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="OnValueChanged"></param>
        public static void CreateTitleHeader(ref PhotoModeUIHelper runtimeUI, Transform parent, string headerText, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, headerText, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabelVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(true);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetHeaderText(headerText);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, headerText);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates dropdown
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="OnValueChanged"></param>
        public static void CreateDropdown(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, int value, UnityAction<int> OnValueChanged, List<string> options, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetDropdown(value, OnValueChanged);
            runtimeUI.SetDropdownOptions(options);
            runtimeUI.SetDropdownVisability(true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            OnValueChanged.Invoke(value);
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates dropdown
        /// </summary>
        /// <param name="toggle"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="OnValueChanged"></param>
        public static void CreateDropdown(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, bool value, UnityAction<int> OnValueChanged, List<string> options, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }

            int intValue = 0;
            if (value)
            {
                intValue = 1;
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetDropdown(intValue, OnValueChanged);
            runtimeUI.SetDropdownOptions(options);
            runtimeUI.SetDropdownVisability(true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            OnValueChanged.Invoke(intValue);
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates label
        /// </summary>
        /// <param name="label"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        public static void CreateLabel(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, string value, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name, 0);
            runtimeUI.SetLabel(value, 1);
            runtimeUI.SetLabelVisible(true, true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates button
        /// </summary>
        /// <param name="label"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="buttonLabel"></param>
        /// <param name="OnClicked"></param>
        public static void CreateButton(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, string buttonLabel, UnityAction OnClicked, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetButton(buttonLabel, OnClicked);
            runtimeUI.SetButtonVisible(true);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates button
        /// </summary>
        /// <param name="label"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="buttonLabel"></param>
        /// <param name="OnClicked"></param>
        public static void CreateColorField(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, Color currentColor, bool hdr, UnityAction OnClicked, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(true);
            runtimeUI.SetColorPreviewImage(currentColor, hdr);
            runtimeUI.SetColorPreviewOnClicked(OnClicked);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates button
        /// </summary>
        /// <param name="label"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="buttonLabel"></param>
        /// <param name="OnClicked"></param>
        public static void CreateBannerImage(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, string searchForImageName, List<PhotoModeImages> profiles, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            PhotoModeImages imageData = GetImageData(searchForImageName, profiles);
            if (imageData != null)
            {
                runtimeUI.SetImage(imageData);
                runtimeUI.SetImageVisability(true);
            }
            else
            {
                runtimeUI.SetImageVisability(false);
            }

            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetLabelVisible(false);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(false);
            m_isSettingValues = false;
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates vector 3
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="onChangedX"></param>
        /// <param name="onChangedY"></param>
        /// <param name="onChangedZ"></param>
        /// <param name="useTooltip"></param>
        /// <param name="tooltipID"></param>
        public static void CreateVector(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, Vector3 value, UnityAction<string> onChangedX, UnityAction<string> onChangedY, UnityAction<string> onChangedZ, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(true);
            runtimeUI.SetVector(value, onChangedX, onChangedY, onChangedZ);
            m_isSettingValues = false;
            onChangedX.Invoke(value.x.ToString(m_floatFormat));
            onChangedY.Invoke(value.y.ToString(m_floatFormat));
            onChangedZ.Invoke(value.z.ToString(m_floatFormat));
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        /// <summary>
        /// Creates vector 2
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="parent"></param>
        /// <param name="Name"></param>
        /// <param name="value"></param>
        /// <param name="onChangedX"></param>
        /// <param name="onChangedY"></param>
        /// <param name="onChangedZ"></param>
        /// <param name="useTooltip"></param>
        /// <param name="tooltipID"></param>
        public static void CreateVector(ref PhotoModeUIHelper runtimeUI, Transform parent, string Name, Vector2 value, UnityAction<string> onChangedX, UnityAction<string> onChangedY, bool useTooltip = false, string tooltipID = "")
        {
            m_isSettingValues = true;
            if (runtimeUI != null)
            {
                if (Application.isPlaying)
                {
                    GameObject.Destroy(runtimeUI.gameObject);
                }
                else
                {
                    GameObject.DestroyImmediate(runtimeUI.gameObject);
                }
            }
            runtimeUI = CreateChild(m_runtimeUIPrefab, Name, parent);
            runtimeUI.m_isUsingSlider = true;
            runtimeUI.SetLabel(Name);
            runtimeUI.SetLabelVisible(true);
            runtimeUI.SetToggleVisible(false);
            runtimeUI.SetSliderVisible(false);
            runtimeUI.SetInputVisible(false);
            runtimeUI.SetButtonVisible(false);
            runtimeUI.SetDropdownVisability(false);
            runtimeUI.SetImageVisability(false);
            runtimeUI.SetHeaderVisible(false);
            runtimeUI.SetColorPreviewImageVisable(false);
            runtimeUI.SetVectorVisable(true, RuntimeUIVectorMode.Vector2);
            runtimeUI.SetVector(value, onChangedX, onChangedY);
            m_isSettingValues = false;
            onChangedX.Invoke(value.x.ToString(m_floatFormat));
            onChangedY.Invoke(value.y.ToString(m_floatFormat));
            AddTooptipToObject(runtimeUI.gameObject, useTooltip, tooltipID, Name);
            PhotoMode.CurrentRuntimeUIElements.Add(runtimeUI);
        }
        ///<summary>Removes all children of given GameObject except those in exceptions list</summary>
        ///<param name="parent">Transform to clear</param>
        ///<param name="exceptions">List of children to keep</param>
        public static void RemoveAllChildren(Transform parent, List<Transform> exceptions)
        {
            if (parent == null)
            {
                return;
            }
            Queue<GameObject> children = new Queue<GameObject>(parent.childCount);
            for (int x = 0; x < parent.childCount; ++x)
            {
                Transform child = parent.GetChild(x);
                if (exceptions.Contains(child))
                    continue;
                children.Enqueue(child.gameObject);
            }

            if (Application.isPlaying)
            {
                while (children.Count > 0)
                {
                    GameObject.Destroy(children.Dequeue());
                }
            }
            else
            {
                while (children.Count > 0)
                {
                    GameObject.DestroyImmediate(children.Dequeue());
                }
            }
        }
        /// <summary>
        /// Updates the input fields
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UpdateWrap(string str)
        {
            int SanityCheck = 9000;

            System.Text.StringBuilder ret = new System.Text.StringBuilder();
            int p = 0;
            while (p < str.Length && --SanityCheck > 0)
            {
                if (p + m_metricsWrapLength >= str.Length)
                {
                    ret.Append(str.Substring(p));
                    break;
                }
                else
                {
                    int editPos = str.LastIndexOf(' ', p + m_metricsWrapLength, m_metricsWrapLength);
                    if (editPos < 0)
                    {
                        editPos = p + m_metricsWrapLength;
                    }

                    ret.AppendLine(str.Substring(p, editPos - p));
                    p = editPos + 1;
                }
            }
            return ret.ToString();
        }
        /// <summary>
        /// Gets a sprite from the profiles
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="profiles"></param>
        /// <returns></returns>
        public static PhotoModeImages GetImageData(string imageName, List<PhotoModeImages> profiles)
        {
            if (profiles.Count > 0)
            {
                foreach (PhotoModeImages profile in profiles)
                {
                    if (profile.m_name == imageName)
                    {
                        return profile;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Helpful function to convert a bool to an int
        /// This can be used for dropdowns in UI
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ConvertBoolToInt(bool value)
        {
            int intValue = 0;
            if (value)
            {
                intValue = 1;
            }

            return intValue;
        }
        /// <summary>
        /// Converts an int value to a bool
        /// This can be used for dropdowns in UI
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ConvertIntToBool(int value)
        {
            if (value == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        /// <summary>
        /// Adds the tooltip trigger to a gameobject
        /// </summary>
        /// <param name="uiObject"></param>
        /// <param name="useTooltip"></param>
        /// <param name="tooltipID"></param>
        public static void AddTooptipToObject(GameObject uiObject, bool useTooltip, string tooltipID, string alternativeTooltipID)
        {
            if (uiObject == null)
            {
                return;
            }

            TooltipManager manager = TooltipManager.Instance;
            TooltipTrigger tooltip = uiObject.GetComponent<TooltipTrigger>();
            if (useTooltip)
            {
                if (manager == null)
                {
                    if (UIConfiguration.Instance != null)
                    {
                        UIConfiguration.Instance.CreateTooltipManager();
                    }
                }

                TooltipProfileData profile = null;
                if (string.IsNullOrEmpty(tooltipID))
                {
                    profile = TooltipManager.Instance.GetTooltip(alternativeTooltipID);
                }
                else
                {
                    profile = TooltipManager.Instance.GetTooltip(tooltipID);
                }

                if (profile != null)
                {
                    if (!string.IsNullOrEmpty(profile.m_text))
                    {
                        if (tooltip == null)
                        {
                            tooltip = uiObject.AddComponent<TooltipTrigger>();
                        }

                        tooltip.m_tooltipHeader = profile.m_header;
                        tooltip.m_tooltipContent = profile.m_text;
                    }
                }
            }
            else
            {
                if (tooltip != null)
                {
                    GameObject.DestroyImmediate(tooltip);
                }
            }
        }

        #endregion
        #region Private

        /// <summary>creates a a copy of prefab as a child of parent with a RectTransform Component and a Component of given Type</summary>
        /// <typeparam name="T">Component Type of prefab</typeparam>
        /// <param name="prefab">Prefab to instantiate</param>
        /// <param name="Name">name of GameObject</param>
        /// <param name="parentGO">Transform to use as parent</param>
        /// <returns>reference to requested Component</returns>
        private static T CreateChild<T>(T prefab, string Name, Transform parent) where T : Component
        {
            T retVal = GameObject.Instantiate(prefab);
            if (retVal == null)
            {
                return null;
            }

            GameObject go = retVal.gameObject;
            go.name = Name;
            go.SetActive(true);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            RectTransform rt = go.transform as RectTransform;
            RectTransform prt = parent as RectTransform;
            if (rt && prt)
            {
                rt.anchoredPosition = prt.anchoredPosition;
                rt.anchorMax = prt.anchorMax;
                rt.anchorMin = prt.anchorMin;
            }
            return retVal;
        }

        #endregion

        #endregion
        #region Profile Loading

        /// <summary>
        /// Loads a photo mode profile by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static PhotoModeProfile LoadPhotoModeProfile(string name, List<PhotoModeProfile> profiles)
        {
            if (profiles.Count > 0)
            {
                for (int i = 0; i < profiles.Count; i++)
                {
                    if (profiles[i].name == name)
                    {
                        return profiles[i];
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// Loads a photo mode profile by index
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public static PhotoModeProfile LoadPhotoModeProfile(int idx, List<PhotoModeProfile> profiles)
        {
            if (profiles.Count > 0)
            {
                if (idx < 0 || idx > profiles.Count - 1)
                {
                    int count = profiles.Count - 1;
                    Debug.LogError("The idx was out of range please provide an id from 0 to " + count);
                    return null;
                }

                for (int i = 0; i < profiles.Count; i++)
                {
                    if (i == idx)
                    {
                        return profiles[i];
                    }
                }
            }

            return null;
        }

        #endregion
        #region Utils

        /// <summary>
        /// Converts vector 4 into a color this is useful when using HDR colors
        /// RGB = color
        /// A = HDR intensity
        /// </summary>
        /// <param name="vector4"></param>
        /// <returns></returns>
        public static Color ConvertVector4ToColor(Vector4 vector4)
        {
            return new Color(vector4.x, vector4.y, vector4.z, vector4.w);
        }
        /// <summary>
        /// Converts vector 4 into a color this is useful when using HDR colors
        /// RGB = color
        /// A = HDR intensity
        /// </summary>
        /// <param name="vector4"></param>
        /// <returns></returns>
        public static Vector4 ConvertColorToVector4(Color color)
        {
            return new Vector4(color.r, color.g, color.b, color.a);
        }
        /// <summary>
        /// Function used to set the slider value of UI and input field
        /// The input field will only be set is the user is using the slider
        /// </summary>
        /// <param name="runtimeUI"></param>
        /// <param name="value"></param>
        public static void SetSliderValue(PhotoModeUIHelper runtimeUI, float value)
        {
            if (runtimeUI != null)
            {
                runtimeUI.SetValue(value, runtimeUI.m_isUsingSlider);
                runtimeUI.m_isUsingSlider = false;
            }
        }
        /// <summary>
        /// Function used to get float out of string then calls the 'SetAction' while clamping the output float value based on (clampMinMax)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="setAction"></param>
        /// <param name="clampMin"></param>
        /// <param name="clampMax"></param>
        public static void GetAndSetFloatValue(string value, UnityAction<float> setAction, Vector2 clampMinMax, PhotoModeUIHelper runtimeUI)
        {
            if (runtimeUI != null)
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "0";
                }

                if (float.TryParse(value, out float f))
                {
                    if (f < clampMinMax.x || f > clampMinMax.y)
                    {
                        SetIsUsingSliderValue(runtimeUI);
                    }

                    setAction.Invoke(Mathf.Clamp(f, clampMinMax.x, clampMinMax.y));
                }
            }
        }
        /// <summary>
        /// Function that can use used when refreshing a runtime UI element
        /// </summary>
        /// <param name="runtimeUI"></param>
        /// <param name="value"></param>
        public static void SetIsUsingSliderValue(PhotoModeUIHelper runtimeUI, bool value = true)
        {
            if (runtimeUI != null)
            {
                runtimeUI.m_isUsingSlider = value;
            }
        }
        /// <summary>
        /// Sets min/max values
        /// </summary>
        /// <param name="values"></param>
        public static void SetNewMinMaxValuesInPhotoMode(PhotoModeMinAndMaxValues values)
        {
            PhotoMode photoMode = PhotoMode.Instance;
            if (photoMode != null)
            {
                if (values == null)
                {
                    return;
                }
                else
                {
                    photoMode.SetMinMax(values);
                }
            }
        }

        #endregion
    }
}