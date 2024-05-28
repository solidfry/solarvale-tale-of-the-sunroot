using System;
using PWCommon5;
using UnityEngine;
using UnityEditor;

namespace Gaia
{
    public class AdvancedWaterUtilsEditor
    {
        #region Editor Utils

        /// <summary>
        /// Draws the frustum editor
        /// </summary>
        /// <param name="m_editorUtils"></param>
        /// <param name="helpEnabled"></param>
        /// <param name="useFrustum"></param>
        /// <param name="worldSize"></param>
        /// <param name="worldOrigin"></param>
        /// <param name="resolution"></param>
        /// <param name="percentage"></param>
        /// <param name="debug"></param>
        /// <param name="drawGizmos"></param>
        /// <param name="buildCullingGroups"></param>
        public static void DrawFrustumCheckEdtior(EditorUtils m_editorUtils, UnityEngine.Object editorObject, bool helpEnabled, ref FrustumData data, Func<int> buildCullingGroups)
        {
            data.m_useFrustumCalculation = m_editorUtils.Toggle("UseFrustumCheck", data.m_useFrustumCalculation, helpEnabled);
            if (data.m_useFrustumCalculation)
            {
                EditorGUILayout.HelpBox("Frustum Check is an experimental feature and could cause some issues. If your ocean is not refreshing the reflections disable this feature.", MessageType.Info);
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                data.m_worldSize = m_editorUtils.IntField("WorldSize", data.m_worldSize, helpEnabled);
                data.m_worldOrigin = m_editorUtils.Vector2Field("WorldOrigin", data.m_worldOrigin, helpEnabled);
                data.m_cullingGroupResolution = m_editorUtils.IntField("FrustumResolution", data.m_cullingGroupResolution, helpEnabled);
                data.m_percentageCount = m_editorUtils.Slider("ActivePercentage", data.m_percentageCount, 0.1f, 100f, helpEnabled);
                EditorGUILayout.LabelField("Debugging");
                EditorGUI.indentLevel++;
                data.m_debug = m_editorUtils.Toggle("ShowDebugs", data.m_debug, helpEnabled);
                data.m_drawGizmos = m_editorUtils.Toggle("ShowGizmos", data.m_drawGizmos, helpEnabled);
                if (data.m_drawGizmos)
                {
                    EditorGUILayout.HelpBox("Having gizmos enabled could really slow your editor. If your editor is lagging turn off 'Draw Gizmos'.", MessageType.Warning);
                }
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                if (EditorGUI.EndChangeCheck())
                {
                    if (Application.isPlaying)
                    {
                        buildCullingGroups();
                    }

                    if (editorObject != null)
                    {
                        EditorUtility.SetDirty(editorObject);
                    }
                }
            }
        }

        #endregion
    }
}