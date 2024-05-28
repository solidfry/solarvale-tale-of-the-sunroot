using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{
    [CustomEditor(typeof(GaiaTask))]
    public class GaiaTaskEditor : Editor
    {
        private GaiaTask m_taskManager;
        private GUIStyle m_boxStyle;

        public override void OnInspectorGUI()
        {
            m_taskManager = (GaiaTask) target;
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
            EditorGUILayout.LabelField("Current Task Count: " + m_taskManager.m_currentTasksInQueue);
            if (m_taskManager.m_taskSystemRunning)
            {
                EditorGUILayout.LabelField("Is Task System Running? YES");
            }
            else
            {
                EditorGUILayout.LabelField("Is Task System Running? NO");
            }

            EditorGUILayout.BeginVertical(m_boxStyle);
            EditorGUILayout.LabelField("Task List", EditorStyles.boldLabel);
            for (int i = 0; i < m_taskManager.Tasks.Count; i++)
            {
                GaiaTaskBase task = m_taskManager.Tasks[i];
                if (task != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Task ID [" + i + "]");
                    EditorGUILayout.LabelField(task.ToString());
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

        }
    }
}