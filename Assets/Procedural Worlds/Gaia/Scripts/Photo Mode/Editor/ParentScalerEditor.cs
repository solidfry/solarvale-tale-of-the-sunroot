using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{
    [CustomEditor(typeof(ParentScaler))]
    public class ParentScalerEditor : Editor
    {
        private GUIStyle m_boxStyle;
        private ParentScaler m_scaler;

        public override void OnInspectorGUI()
        {
            m_scaler = (ParentScaler) target;

            //Set up the box style
            if (m_boxStyle == null)
            {
                m_boxStyle = new GUIStyle(GUI.skin.box)
                {
                    normal = {textColor = GUI.skin.label.normal.textColor},
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.UpperLeft
                };
            }

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Setup", EditorStyles.boldLabel);
            m_scaler.m_scaleWithCanvas = EditorGUILayout.Toggle(new GUIContent("Scale With Canvas", "If enabled any objects in Rects To Scale will scale with the canvas height only."), m_scaler.m_scaleWithCanvas);
            if (m_scaler.m_scaleWithCanvas)
            {
                EditorGUI.indentLevel++;
                m_scaler.m_canvas = (Canvas)EditorGUILayout.ObjectField(new GUIContent("Canvas", "Canvas that is used to reference the height with and will be used when syncing"), m_scaler.m_canvas, typeof(Canvas), true);
                m_scaler.m_mode = (ParentScaleMode)EditorGUILayout.EnumPopup(new GUIContent("Mode", "Sets how the scaling will effects the transforms"), m_scaler.m_mode);
                if (m_scaler.m_mode == ParentScaleMode.PartScreen)
                {
                    m_scaler.m_maxHeight = EditorGUILayout.FloatField("Max Height", m_scaler.m_maxHeight);
                }
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(m_boxStyle);
                EditorGUILayout.LabelField("Rects To Scale", EditorStyles.boldLabel);
                if (m_scaler.m_rectsToScale.Count > 0)
                {
                    for (int i = 0; i < m_scaler.m_rectsToScale.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        if (m_scaler.m_rectsToScale[i] == null)
                        {
                            m_scaler.m_rectsToScale[i] = (RectTransform)EditorGUILayout.ObjectField("Empty Rect [" + i + "]", m_scaler.m_rectsToScale[i], typeof(RectTransform), true);
                        }
                        else
                        {
                            m_scaler.m_rectsToScale[i] = (RectTransform)EditorGUILayout.ObjectField(m_scaler.m_rectsToScale[i].name + " [" + i + "]", m_scaler.m_rectsToScale[i], typeof(RectTransform), true);
                        }

                        if (GUILayout.Button("Remove", GUILayout.MaxWidth(60f)))
                        {
                            m_scaler.m_rectsToScale.RemoveAt(i);
                            GUIUtility.ExitGUI();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                if (GUILayout.Button("Add New Rect"))
                {
                    m_scaler.m_rectsToScale.Add(null);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();
        }
    }
}