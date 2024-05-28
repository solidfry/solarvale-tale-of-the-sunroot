using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Gaia
{
    public enum ParentScaleMode { FullScreen, PartScreen }

    [ExecuteAlways]
    public class ParentScaler : MonoBehaviour
    {
        public bool m_scaleWithCanvas = true;
        public Canvas m_canvas;
        public List<RectTransform> m_rectsToScale = new List<RectTransform>();
        public ParentScaleMode m_mode = ParentScaleMode.FullScreen;
        public float m_maxHeight = 500f;

        private float m_lastScaleHeight = 0f;
        private RectTransform canvasRectTransform;

        private void OnEnable()
        {
#if UNITY_EDITOR

            if (!Application.isPlaying)
            {
                m_lastScaleHeight = 0f;
                EditorApplication.update -= ProcessScaleWithCanvas;
                EditorApplication.update += ProcessScaleWithCanvas;
            }
#endif
        }
        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= ProcessScaleWithCanvas;
#endif
        }
        private void OnDestroy()
        {
#if UNITY_EDITOR
            EditorApplication.update -= ProcessScaleWithCanvas;
#endif
        }
        private void LateUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ProcessScaleWithCanvas();
        }

        private void ProcessScaleWithCanvas()
        {
            if (m_scaleWithCanvas)
            {
                if (m_canvas != null)
                {
                    if (canvasRectTransform == null)
                    {
                        canvasRectTransform = (RectTransform)m_canvas.transform;
                    }

                    if (m_lastScaleHeight != canvasRectTransform.rect.height || m_lastScaleHeight == 0f)
                    {
                        if (m_rectsToScale.Count > 0)
                        {
                            m_lastScaleHeight = canvasRectTransform.rect.height;
                            foreach (RectTransform rectTransform in m_rectsToScale)
                            {
                                if (rectTransform == null)
                                {
                                    continue;
                                }

                                switch (m_mode)
                                {
                                    case ParentScaleMode.FullScreen:
                                    {
                                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, m_lastScaleHeight);
                                        break;
                                    }
                                    case ParentScaleMode.PartScreen:
                                    {
                                        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, Mathf.Clamp(m_lastScaleHeight, 0.1f, m_maxHeight));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}