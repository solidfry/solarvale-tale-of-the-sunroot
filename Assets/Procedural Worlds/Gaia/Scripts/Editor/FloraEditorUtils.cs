#if FLORA_PRESENT
using ProceduralWorlds.Flora;
using PWCommon5;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Gaia
{
    public class FloraEditorUtils
    {
        public static bool SubscribedToUndo = false;
        private static List<FloraLOD> FloraLODs = new List<FloraLOD>();

        public static bool DisplayFloraLODs(List<FloraLOD> floraLODs, string name, GaiaConstants.SpawnerResourceType resourceType, EditorUtils editorUtils, bool helpEnabled)
        {
            if (!SubscribedToUndo)
            {
                SubscribeToUndo(floraLODs);
            }

            if (floraLODs.Count == 0)
            {
                floraLODs.Clear();
                FloraUtils.AddNewDetailerSettingsObject(floraLODs, name, resourceType);
            }

            for (int i = 0; i < floraLODs.Count; i++)
            {
                FloraLOD floraLOD = floraLODs[i];

                if (floraLOD.DetailerSettingsObject == null)
                {
                    //FloraUtils.AddNewDetailerSettingsObject(floraLODs, name, resourceType);
                    floraLOD.DetailerSettingsObject = ScriptableObject.CreateInstance<DetailScriptableObject>();
                    FloraUtils.SaveSettingsFile(floraLOD.DetailerSettingsObject, ref floraLOD.m_detailerSettingsObjectAssetGUID, ref floraLOD.m_detailerSettingsObjectInstanceID, false, floraLOD.m_name, GaiaDirectories.GetFloraDataPath());

                }

                EditorGUILayout.BeginHorizontal();
                {
                    floraLOD.m_foldedOut = editorUtils.Foldout(floraLOD.m_foldedOut, new GUIContent(floraLOD.DetailerSettingsObject.Name, "Fold / Unfold to hide or show the settings for this LOD level for the Flora settings"));
                    GUILayout.FlexibleSpace();
                    if (editorUtils.Button("FloraLODPlusButton", GUILayout.Width(20), GUILayout.Height(18)))
                    {
                        FloraUtils.AddNewDetailerSettingsObject(floraLODs, name, resourceType, i+1);
                    }
                    if (editorUtils.Button("FloraLODMinusButton", GUILayout.Width(20), GUILayout.Height(18)))
                    {
                        floraLODs.RemoveAt(i);
                        if (floraLODs.Count == 0)
                        {

                            if (EditorUtility.DisplayDialog("Deletion of last Flora LOD", "You are deleting the last Flora LOD for this object, do you want to reset that single LOD with the default settings, or do you want to completely remove the flora config and disable flora for this resource?", "Reset to Default", "Remove completely"))
                            {
                                FloraUtils.AddNewDetailerSettingsObject(floraLODs, name, resourceType);
                            }
                            else
                            {
                                FloraUtils.RemoveSettingsSOFromAllTerrains(floraLODs);
                                floraLODs.Clear();
                                return false;
                            }
                        }
                    }
                    if (i > 0)
                    {
                        if (editorUtils.Button("FloraLODUpButton", GUILayout.Width(20), GUILayout.Height(18)))
                        {
                            FloraLOD tmp = floraLODs[i - 1];
                            floraLODs[i - 1] = floraLODs[i];
                            floraLODs[i - 1].m_index = i - 1;
                            floraLODs[i] = tmp;
                            floraLODs[i].m_index = i;
                        }
                    }
                    else
                    {
                        GUILayout.Space(23);
                    }
                    if (i < floraLODs.Count - 1)
                    {
                        if (editorUtils.Button("FloraLODDownButton", GUILayout.Width(20), GUILayout.Height(18)))
                        {
                            FloraLOD tmp = floraLODs[i + 1];
                            floraLODs[i + 1] = floraLODs[i];
                            floraLODs[i + 1].m_index = i + 1;
                            floraLODs[i] = tmp;
                            floraLODs[i].m_index = i;
                        }
                    }
                    else
                    {
                        GUILayout.Space(23);
                    }
                }
                EditorGUILayout.EndHorizontal();
                if (floraLOD.m_foldedOut)
                {
                    DetailScriptableObject oldObject = floraLOD.DetailerSettingsObject;

                    bool currentGUIState = GUI.enabled;

                    if (Application.isPlaying)
                    {
                        GUI.enabled = false;
                    }

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(editorUtils.GetContent("DetailPWGrassSettingsObject"), GUILayout.Width(EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15));
                    floraLOD.DetailerSettingsObject = (DetailScriptableObject)EditorGUILayout.ObjectField(floraLOD.DetailerSettingsObject, typeof(DetailScriptableObject), false);

                    if (floraLOD.DetailerSettingsObject != null && oldObject != null && oldObject != floraLOD.DetailerSettingsObject)
                    {
                        if (EditorUtility.DisplayDialog("Work with the original settings object?", "It looks like you assigned a Detail Settings object to this spawner. The spawner can directly work with the file you just assigned. If you then change settings like color etc. those will be changed in the original file as well. \r\n Alternatively, the spawner can copy the settings over into the already existing settings object that was used before. In this case you would not overwrite the original file when changing settings.\r\n\r\n Do you want to copy the settings over, or rather work directly with the original file?", "Copy Settings", "Work With Original"))
                        {
                            floraLOD.CopySettingsAndApply(oldObject);
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("Update terrains with new settings file?", "You have just swapped to a new settings file to work with. Do you want to swap out the old detail setting on all terrains in your scene as well? This will update the details that were spawned while using the old file to the new settings.", "Yes, update terrains", "No, keep terrains"))
                            {
                                FloraUtils.UpdateAllTerrainsWithNewSettingsSO(oldObject, floraLOD.DetailerSettingsObject);
                            }
                        }
                    }

                    if (GUILayout.Button(editorUtils.GetContent("DetailPWGrassSettingsNewButton"), GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("Create new settings file?", "Do you want to create a new settings file with the default settings?", "Create new file", "Cancel"))
                        {
                            FloraUtils.AddNewDetailerSettingsObject(floraLODs, name, resourceType);
                        }
                    }

                    EditorGUILayout.EndHorizontal();

                    GUI.enabled = currentGUIState;

                    editorUtils.InlineHelp("DetailPWGrassSettingsObject", helpEnabled);

                    EditorGUI.BeginChangeCheck();

                    FloraEditorUtility.HelpEnabled = helpEnabled;
                    if (floraLOD.DetailerSettingsObject != null)
                    {
                        FloraEditorUtility.DetailerEditor(floraLOD.DetailerSettingsObject);
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        //During runtime we need to inform the detail object instances that the settings object has been updated
                        if (Application.isPlaying)
                        {
                            foreach (DetailObject detailObject in Resources.FindObjectsOfTypeAll<DetailObject>())
                            {
                                if (detailObject.DetailScriptableObject == floraLOD.DetailerSettingsObject)
                                {
                                    detailObject.RefreshAll();
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public static void SubscribeToUndo(List<FloraLOD> floraLODs)
        {
            FloraLODs = floraLODs;
            if (FloraLODs.Count < 1)
            {
                Debug.Log("No Flora LODS found, undo buffer will not be registered");
            }
            else
            {
                Undo.undoRedoPerformed -= UndoBuffer;
                Undo.undoRedoPerformed += UndoBuffer;
                SubscribedToUndo = true;
            }
        }
        public static void UnSubscribeToUndo()
        {
            Undo.undoRedoPerformed -= UndoBuffer;
            SubscribedToUndo = false;
        }
        private static void UndoBuffer()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (FloraLODs != null && FloraLODs.Count > 0)
            {
                foreach (FloraLOD floraLod in FloraLODs)
                {
                    foreach (DetailObject detailObject in Resources.FindObjectsOfTypeAll<DetailObject>())
                    {
                        if (detailObject.DetailScriptableObject == floraLod.DetailerSettingsObject)
                        {
                            detailObject.RefreshAll();
                        }
                    }
                }
            }
        }
    }
}
#endif