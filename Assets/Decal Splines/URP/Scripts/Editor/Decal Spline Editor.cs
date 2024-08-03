using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace DecalSplines
{
    //Class used to extend the unity editor in order to be able to place spline segments etc..
    [CustomEditor(typeof(DecalSpline))]
    public class DecalSplineEditor : Editor
    {
        private bool placeMode = false;
        private SerializedProperty activeTheme;
        private SerializedProperty projectionDepth;
        private SerializedProperty autoSnap;
        private SerializedProperty liveUpdate;
        private SplineThemeEditor themeEditor;
        private bool prevLeftMouseDown;

        private void OnEnable()
        {
            activeTheme = serializedObject.FindProperty("activeTheme");
            projectionDepth = serializedObject.FindProperty("projectionDepth");
            autoSnap = serializedObject.FindProperty("autoSnap");
            liveUpdate = serializedObject.FindProperty("liveUpdate");
        }

        //Draw the property window
        public override void OnInspectorGUI()
        {
            Event e = Event.current;
            ProcessEvent(e);

            DecalSpline decalSpline = (DecalSpline)target;
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            //The button to go into place mode
            if (GUILayout.Button("Place", GUILayout.Width(120f)))
            {
                if (activeTheme.objectReferenceValue != null)
                    placeMode = true;
            }

            GUILayout.Label("Render Mask");
            GUILayoutOption[] guiOptions = null;
            string[] options = GraphicsSettings.defaultRenderPipeline.renderingLayerMaskNames;
            uint renderLayerMask = serializedObject.FindProperty("renderLayerMask").uintValue;

            EditorGUI.BeginChangeCheck();
            renderLayerMask = (uint)EditorGUILayout.MaskField((int)renderLayerMask, options, guiOptions);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.FindProperty("renderLayerMask").uintValue = renderLayerMask;
                serializedObject.ApplyModifiedProperties();
                decalSpline.UpdateDecalSpline();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            //The button to snap down the spline
            if (GUILayout.Button("Snap", GUILayout.Width(120f)))
            {
                decalSpline.Snap();
                decalSpline.UpdateDecalSpline();
            }
            EditorGUILayout.PropertyField(autoSnap);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            //Button that updates the Decal Spline.
            if (GUILayout.Button("Update", GUILayout.Width(120f)))
            {
                decalSpline.UpdateDecalSpline();
            }
            EditorGUILayout.PropertyField(liveUpdate);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            //Button that clears the Decal Spline
            if (GUILayout.Button("Clear All", GUILayout.Width(120f)))
            {
                //Comfirmation Window to prevent accidental clearing
                if (EditorUtility.DisplayDialog("Clear Decal Spline", "Confirm that you want to permanetly clear the selected Decal spline.", "Confirm"))
                {
                    decalSpline.ClearDecalSpline();
                    serializedObject.Update();
                }
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(projectionDepth);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                decalSpline.UpdateDecalSpline();
            }
            EditorGUILayout.EndHorizontal();

            //The style selection box as nested editor

            EditorGUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Theme Settings:");
            GUILayout.EndHorizontal();

            //The active theme selection
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(activeTheme);
            if (EditorGUI.EndChangeCheck())
            {
                themeEditor = null;
            }
            if (activeTheme.objectReferenceValue != null)
            {
                ThemeEditor();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void ThemeEditor()
        {
            if (themeEditor == null)
            {
                themeEditor = (SplineThemeEditor)CreateEditor(activeTheme.objectReferenceValue as SplineTheme);
                themeEditor.OnActiveStyleChanged += ThemeActiveStyleChanged;
            }

            themeEditor.OnInspectorGUI();
        }

        public void OnSceneGUI()
        {
            DecalSpline decalSpline = (DecalSpline)target;

            Event e = Event.current;
            ProcessEvent(e);

            if (placeMode)
            {
                //Escape to exit place mode.
                if (e.isKey && e.keyCode == KeyCode.Escape)
                {
                    placeMode = false;
                }

                //Check the mouse3DPos and draw the placement gizmo.
                Vector3 mousePos3D;
                if (EditorInput.MousePosition3D(HandleUtility.GUIPointToScreenPixelCoordinate(e.mousePosition), decalSpline.transform, out mousePos3D))
                {
                    SplineTheme theme = (SplineTheme)(activeTheme.objectReferenceValue);
                    if (theme != null)
                    {
                        ISplineStyle style = theme.ActiveStyle;
                        float gizmoSize = 0.1f;
                        if (style != null)
                            gizmoSize = style.Width * 0.5f;
                        Handles.DrawWireDisc(mousePos3D, decalSpline.transform.rotation * Vector3.up  , gizmoSize);//draw placement gizmo.

                        //Place a spline segment if left mouse button clicked .
                        if (e.type == EventType.MouseUp && e.button == 0)
                        {
                            decalSpline.AddSegment(mousePos3D, style);
                        }
                    }
                    else placeMode = false;
                }

                //Prevent deselection.
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
            }

            CheckTransfromChanged(decalSpline);
            DrawProjectionGizmo(decalSpline);
            decalSpline.DrawGizmos();

            if (prevLeftMouseDown && !EditorInput.LeftMouseDown)
                decalSpline.UpdateDecalSpline();

            prevLeftMouseDown = EditorInput.LeftMouseDown;

            //keep the editor refreshing as long as Decal Spline is selected.
            SceneView.RepaintAll();
        }

        private void ProcessEvent(Event e)
        {
            if (e.type == EventType.MouseDown && e.button == 0)
                EditorInput.LeftMouseDown = true;
            if (e.type == EventType.MouseUp && e.button == 0)
                EditorInput.LeftMouseDown = false;

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
                EditorInput.SpaceDown = true;
            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.Space)
                EditorInput.SpaceDown = false;

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftControl)
                EditorInput.CTRLDown = true;
            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.LeftControl)
                EditorInput.CTRLDown = false;

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.LeftShift)
                EditorInput.ShiftDown = true;
            if (e.type == EventType.KeyUp && e.keyCode == KeyCode.LeftShift)
                EditorInput.ShiftDown = false;
        }

        private void DrawProjectionGizmo(DecalSpline decalSpline)
        {
            Transform targetTransform = decalSpline.transform;
            Handles.ArrowHandleCap(0, targetTransform.position, targetTransform.rotation* Quaternion.LookRotation(Vector3.down),.5f , EventType.Repaint);
            Handles.RectangleHandleCap(0, targetTransform.position, targetTransform.rotation * Quaternion.LookRotation(Vector3.down),1f  , EventType.Repaint);
        }

        private Vector3 prevPos;
        private Quaternion prevRot;
        private void CheckTransfromChanged(DecalSpline decalSpline)
        {
            if (!EditorInput.LeftMouseDown)
            {
                if (autoSnap.boolValue != EditorInput.ShiftDown)
                    if(prevPos != Vector3.zero)
                        if (decalSpline.transform.position != prevPos || decalSpline.transform.rotation != prevRot)
                            decalSpline.Snap();
                prevPos = decalSpline.transform.position;
                prevRot = decalSpline.transform.rotation;
            }
        }
        

        private void ThemeActiveStyleChanged(object sender, EventArgs e)
        {
            //Activate place mode if the style was changed for user friendly behavior.
            if (activeTheme.objectReferenceValue != null)
                placeMode = true;
        }


        /* //Find the mouse position in 3D.
        private bool MousePosition3D(Vector2 mousePos, DecalSpline decalSpline, out Vector3 position3D)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            RaycastHit hitInfo;
            bool result = Physics.Raycast(ray, out hitInfo, 1000f, ~0, QueryTriggerInteraction.Ignore);

            position3D = hitInfo.point;

            Vector3 snapPos = Vector3.zero;
            bool result = false;
            RaycastHit[] raycastHits = Physics.RaycastAll(ray, 2000f);
            if (raycastHits != null && raycastHits.Length > 0)
            {
                float closestDistance = float.MaxValue;
                foreach (RaycastHit hit in raycastHits)
                {
                    if (hit.distance <= closestDistance)
                    {
                        if (!hit.transform.IsChildOf(decalSpline.transform))
                        {
                            closestDistance = hit.distance;
                            snapPos = hit.point;
                            result = true;
                        }
                    }
                }
            }

            position3D = snapPos;
            return result;
        }*/

        //Menu item in the Hierarchy window.
        [MenuItem("GameObject/Uhm..Uhm.. Games/Decal Spline")]
        public static void CreateObject(MenuCommand menuCommand)
        {
            GameObject decalSpline = ObjectFactory.CreateGameObject("Decal Spline", typeof(DecalSpline));

            SceneView lastView = SceneView.lastActiveSceneView;
            decalSpline.transform.position = lastView ? lastView.pivot : Vector3.zero;

            StageUtility.PlaceGameObjectInCurrentStage(decalSpline);
            GameObjectUtility.EnsureUniqueNameForSibling(decalSpline);

            Undo.RegisterCreatedObjectUndo(decalSpline, $"Create Object: {decalSpline.name}");
            Selection.activeGameObject = decalSpline;


            EditorGUIUtility.SetIconForObject(decalSpline, EditorGUIUtility.FindTexture("sv_label_5"));

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        //Menu item in the Create Toolbar in the Project window.
        [MenuItem("Assets/Create/Decal Splines/Material")]
        public static void CreateMaterial()
        {
            //Search for the template material.
            string fileName = "Decal Spline Template Material";
            string searchFilter = $"\"{fileName}\"";
            string[] guids = AssetDatabase.FindAssets(searchFilter);

            if (guids.Length > 0)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                if (assetPath.Length > 0)
                {
                    //find the template material
                    Material template = (Material)AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material));

                    if (template != null)
                    {
                        //Copy the template material.
                        Material newMarterial = new Material(template);

                        //Create asset in selected folder.
                        string directory = AssetDatabase.GetAssetPath(Selection.activeObject);
                        if (System.IO.Path.HasExtension(directory))
                            directory = System.IO.Path.GetDirectoryName(directory);

                        string assetName = "New Decal Spline Material.mat";
                        string path = System.IO.Path.Combine(directory, assetName);

                        AssetDatabase.CreateAsset(newMarterial, path);
                        Selection.activeObject = newMarterial;
                    }
                }
            }
        }
    }
}

