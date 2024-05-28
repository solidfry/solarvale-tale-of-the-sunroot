using PWCommon5;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Gaia.GaiaConstants;

#if !GAIA_PRO_PRESENT

namespace Gaia
{
    //public enum CurrentlyViewing { Unknown, Terrain, WorldMap }

    [InitializeOnLoad]
    class Gaia2TopPanel
    {
        static Gaia2TopPanel()
        {
            SceneView.duringSceneGui -= RenderSceneGUI;
            SceneView.duringSceneGui += RenderSceneGUI;
            m_lastActiveScene = SceneManager.GetActiveScene();
            CheckForSessionManager();
        }

        private static void CheckForSessionManager()
        {
            if (SessionManager != null)
            {
                m_sessionManagerExits = true;
            }
            else
            {
                m_sessionManagerExits = false;
            }
        }

        public static bool m_sessionManagerExits = false;
        private static Scene m_lastActiveScene;

        private static GUIStyle GUIStylePanel;
        private static GUIStyle GUIStyleHeader;
        private static GUIStyle m_smallButtonStyle;

        private static Texture2D m_helpButtonImage;


        private static GaiaSessionManager m_sessionManager;
        private static Vector2 m_terrainOpScrollPos;
        //private static EditorUtils m_editorUtils;
        private static GaiaSessionManager SessionManager {
            get {
                if (m_sessionManager == null)
                {
                    m_sessionManager = GaiaSessionManager.GetSessionManager(false,false);
                    if (m_sessionManager == null)
                    {
                        m_sessionManagerExits = false;
                    }
                }
                return m_sessionManager;
            }
        }

        private static GameObject m_worldDesigner;
        private static GameObject WorldDesigner
        {
            get
            {
                if (m_worldDesigner == null)
                {
                    m_worldDesigner = GaiaUtils.GetOrCreateWorldDesigner(false, false);
                }
                return m_worldDesigner;
            }
        }

        private static GaiaSettings m_gaiaSettings;
        private static GaiaSettings GaiaSettings
        {
            get
            {
                if (m_gaiaSettings == null)
                {
                    m_gaiaSettings = GaiaUtils.GetGaiaSettings();
                }
                return m_gaiaSettings;
            }
        }

        private static Terrain m_worldMapTerrain;

        private static Terrain WorldMapTerrain
        {
            get
            {
                if (m_worldMapTerrain == null)
                {
                    m_worldMapTerrain = TerrainHelper.GetWorldMapTerrain();
                }
                return m_worldMapTerrain;
            }
        }

        public static void RenderSceneGUI(SceneView sceneview)
        {
            if (m_lastActiveScene != SceneManager.GetActiveScene())
            {
                CheckForSessionManager();
                m_lastActiveScene = SceneManager.GetActiveScene();
            }

            if (!m_sessionManagerExits)
            {
                return;
            }
            if (SessionManager == null)
            {
                return;
            }

            if (m_helpButtonImage == null)
            {
                m_helpButtonImage = Resources.Load("helpBtnOffp" + PWConst.VERSION_IN_FILENAMES) as Texture2D;
            }

            if (m_smallButtonStyle == null)
            {
                m_smallButtonStyle = new GUIStyle(GUI.skin.button);
                m_smallButtonStyle.padding = new RectOffset(0, 0, 0, 0);
            }

            if (GUIStyleHeader == null)
            {
                GUIStyleHeader = new GUIStyle(GUI.skin.label);
                GUIStyleHeader.alignment = TextAnchor.MiddleCenter;
                GUIStyleHeader.fontStyle = FontStyle.Bold;
            }

            if (GUIStylePanel == null)
            {
                GUIStylePanel = new GUIStyle();
                GUIStylePanel.padding = new RectOffset(0, 0, 0, 0);
                GUIStylePanel.margin = new RectOffset(0, 0, 0, 0);
                GUIStylePanel.alignment = TextAnchor.MiddleCenter;
                GUIStylePanel.border = new RectOffset(2, 2, 2, 2);
                GUIStylePanel.imagePosition = ImagePosition.ImageOnly;
                if (EditorGUIUtility.isProSkin)
                {
                    GUIStylePanel.normal.background = GaiaSettings.m_originUIProBackgroundPro;
                    GUIStylePanel.normal.textColor = Color.white;
                }
                else
                {
                    GUIStylePanel.normal.background = GaiaSettings.m_originUIBackground;
                    GUIStylePanel.normal.textColor = Color.black;
                }
            }

            Handles.BeginGUI();

            if (TerrainLoaderManager.Instance.TerrainSceneStorage.m_hasWorldMap)
            {
                DrawTopPanel();
            }
            Handles.EndGUI();
        }

      

        private static void DrawTopPanel()
        {
            bool currentGUIState = GUI.enabled;

            float scaledScreenWidth = (Camera.current.pixelRect.size.x / EditorGUIUtility.pixelsPerPoint);
            float scaledScreenHeight = (Camera.current.pixelRect.size.y / EditorGUIUtility.pixelsPerPoint);
            

            float sizeX = GaiaSettings.m_gaiaPanelSizeType == PositionType.Relative ? scaledScreenWidth * GaiaSettings.m_gaiaPanelSize.x / 100f : GaiaSettings.m_gaiaPanelSize.x;
            float sizeY = GaiaSettings.m_gaiaPanelSizeType == PositionType.Relative ? scaledScreenHeight * GaiaSettings.m_gaiaPanelSize.y / 100f : GaiaSettings.m_gaiaPanelSize.y;
            float x = GaiaSettings.m_gaiaPanelPositionType == PositionType.Relative ? (scaledScreenWidth * GaiaSettings.m_gaiaPanelPosition.x / 100f) - (sizeX + 50f) / 2f : GaiaSettings.m_gaiaPanelPosition.x;
            float y = GaiaSettings.m_gaiaPanelPositionType == PositionType.Relative ? (scaledScreenHeight * GaiaSettings.m_gaiaPanelPosition.y / 100f) : GaiaSettings.m_gaiaPanelPosition.y;

            GUILayout.BeginArea(new Rect(x, y, sizeX+50, sizeY));
            float leftSpace = 6f;
            EditorGUI.BeginChangeCheck();


            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical(GUIStylePanel, GUILayout.MaxWidth(sizeX));
                {
                    
                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(leftSpace);
                        if (GUILayout.Button("Manager", GUILayout.Height(15), GUILayout.Width(70)))
                        {
                            var manager = EditorWindow.GetWindow<Gaia.GaiaManagerEditor>(false, "Gaia Manager");
                            //Manager can be null if the dependency package installation is started upon opening the manager window.
                            if (manager != null)
                            {
                                Vector2 initialSize = new Vector2(850f, 450f);
                                manager.position = new Rect(new Vector2(Screen.currentResolution.width / 2f - initialSize.x / 2f, Screen.currentResolution.height / 2f - initialSize.y / 2f), initialSize);
                                manager.Show();
                            }
                        }

                        if (WorldDesigner != null)
                        {
                            GUILayout.Label("Gaia", GUIStyleHeader, GUILayout.MaxHeight(16));

                            if (Selection.activeObject == WorldDesigner)
                            {

                                if (!GaiaUtils.HasTerrains())
                                {
                                    GUI.enabled = false;
                                }
                                if (GUILayout.Button(new GUIContent("Show Terrain", "Switches the Scene View to display the actual terrains of your scene."), GUILayout.Height(15), GUILayout.Width(100)))
                                {
                                    TerrainLoaderManager.Instance.SwitchToLocalMap();
                                    Selection.activeObject = null;
                                }
                                GUI.enabled = currentGUIState;
                            }
                            else
                            {
                                if (GUILayout.Button(new GUIContent("Show Designer", "Switches the Scene View to display the high-level world map of the world designer."), GUILayout.Height(15), GUILayout.Width(100)))
                                {
                                    TerrainLoaderManager.Instance.SwitchToWorldMap();
                                }
                            }

                            if (GUILayout.Button(new GUIContent(m_helpButtonImage, "Opens the online help for the scene view panel."), m_smallButtonStyle, GUILayout.Height(15), GUILayout.Width(15)))
                            {
                                Application.OpenURL("https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/30_installation__getting_started/5-using-the-gaia-scene-view-panel-r24/");
                            }

                        }
                        else
                        {
                            GUILayout.Space(-50);
                            GUILayout.Label("Gaia", GUIStyleHeader, GUILayout.MaxHeight(16));
                            if (!TerrainLoaderManager.Instance.ShowLocalTerrain)
                            {
                                TerrainLoaderManager.Instance.SwitchToLocalMap();
                            }
                            if (GUILayout.Button(new GUIContent(m_helpButtonImage, "Opens the online help for the scene view panel."), m_smallButtonStyle, GUILayout.Height(15), GUILayout.Width(15)))
                            {
                                Application.OpenURL("https://canopy.procedural-worlds.com/library/tools/gaia-pro-2021/written-articles/30_installation__getting_started/5-using-the-gaia-scene-view-panel-r24/");
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
#if GAIA_PRO_PRESENT
                    if (SessionManager.m_showSceneViewPanel)
                    {

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Go To Terrain:", GUILayout.Width(100));
                            GUILayout.Label("X", GUILayout.Width(coordLabelWidth));
                            GaiaTerrainLoaderManager.Instance.m_originTargetTileX = EditorGUILayout.DelayedIntField(GaiaTerrainLoaderManager.Instance.m_originTargetTileX, GUILayout.Width(coordInputWidth));
                            GUILayout.Space(84);
                            GUILayout.Label("Z", GUILayout.Width(coordLabelWidth));
                            GaiaTerrainLoaderManager.Instance.m_originTargetTileZ = EditorGUILayout.DelayedIntField(GaiaTerrainLoaderManager.Instance.m_originTargetTileZ, GUILayout.Width(coordInputWidth));
                            if (GUILayout.Button("Go", GUILayout.Height(15), GUILayout.Width(52)))
                            {
                                GaiaTerrainLoaderManager.Instance.SetOriginByTargetTile();
                                range = GaiaTerrainLoaderManager.Instance.GetLoadingRange();
                                currentPos = GaiaTerrainLoaderManager.Instance.GetOrigin();
                            }
                            GUILayout.FlexibleSpace();

                        }
                        EditorGUILayout.EndHorizontal();


                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Selected:", GUILayout.Width(100));
                            if (Selection.activeObject != null)
                            {
                                if (Selection.activeObject.GetType() == typeof(GameObject))
                                {
                                    GameObject selectedObject = (GameObject)Selection.activeObject;
                                    if (selectedObject.scene != null)
                                    {

                                        GUILayout.Label("X", GUILayout.Width(coordLabelWidth));
                                        GUILayout.Label((selectedObject.transform.position.x + currentPos.x).ToString(), GUILayout.Width(coordInputWidth));
                                        GUILayout.FlexibleSpace();
                                        GUILayout.Label("Y", GUILayout.Width(coordLabelWidth));
                                        GUILayout.Label((selectedObject.transform.position.y + currentPos.y).ToString(), GUILayout.Width(coordInputWidth));
                                        GUILayout.FlexibleSpace();
                                        GUILayout.Label("Z", GUILayout.Width(coordLabelWidth));
                                        GUILayout.Label((selectedObject.transform.position.z + currentPos.z).ToString(), GUILayout.Width(coordInputWidth));
                                        GUI.enabled = !Application.isPlaying;
                                        if (GUILayout.Button("Center", GUILayout.Height(15)))
                                        {
                                            currentPos = new Vector3Double(selectedObject.transform.position.x + currentPos.x, selectedObject.transform.position.y + currentPos.y, selectedObject.transform.position.z + currentPos.z);
                                            bool needsShift = true;

                                            //Check if we should shift the selected object around as well
                                            if (selectedObject.scene != SessionManager.gameObject.scene)
                                            {
                                                //in a separate scene from session manager -> should be shifted automatically
                                                needsShift = false;
                                            }

                                            if (needsShift && selectedObject.transform.GetComponentInParent<Terrain>() != null)
                                            {
                                                //child of a terrain -> should be shifted automatically
                                                needsShift = false;
                                            }

                                            if (needsShift && selectedObject.transform.GetComponentInParent<FloatingPointFixMember>() != null)
                                            {
                                                //is child of a floating point fix member -> should be shifted automatically
                                                needsShift = false;
                                            }

                                            if (needsShift)
                                            {
                                                //still needs to be shifted -> ask the user if they wish to add a floating point fix 
                                                if (EditorUtility.DisplayDialog("Shift centered object?", "You are about to center on an object that does not seem to be set up to be shifted together with the world origin. Do you want to add a Floating Point Fix Member component on this object so that it will be shifted with the world, or do you want the object not to be affected by the shift?", "Add component", "Leave object in place"))
                                                {
                                                    selectedObject.AddComponent<FloatingPointFixMember>();
                                                }
                                            }
                                        }
                                        GUI.enabled = currentGUIState;
                                    }
                                }
                            }
                            GUILayout.FlexibleSpace();

                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            GUI.enabled = !Application.isPlaying;
                            GUILayout.Label("Origin:", GUILayout.Width(100));
                            GUILayout.Label("X", GUILayout.Width(coordLabelWidth));
                            currentPos.x = EditorGUILayout.DelayedDoubleField(currentPos.x, GUILayout.Width(coordInputWidth));
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Y", GUILayout.Width(coordLabelWidth));
                            currentPos.y = EditorGUILayout.DelayedDoubleField(currentPos.y, GUILayout.Width(coordInputWidth));
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Z", GUILayout.Width(coordLabelWidth));
                            currentPos.z = EditorGUILayout.DelayedDoubleField(currentPos.z, GUILayout.Width(coordInputWidth));
                            GUILayout.Space(56);
                            GUI.enabled = currentGUIState;

                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = !Application.isPlaying;
                        {
                            GUILayout.Label("Loading Range:", GUILayout.Width(115));
                            range = EditorGUILayout.DelayedDoubleField(range, GUILayout.Width(60));
                            GUILayout.Space(25);
                            SessionManager.m_showOriginLoadingBounds = EditorGUILayout.Toggle("Show Loading Range", SessionManager.m_showOriginLoadingBounds);
                        }
                        GUI.enabled = currentGUIState;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("Show Loader Manager...", GUILayout.Height(15), GUILayout.Width(180)))
                            {
                                GameObject loaderObj = GaiaUtils.GetTerrainLoaderManagerObject();
                                Selection.activeObject = loaderObj;
                            }
                            GUILayout.Space(25);
                            SessionManager.m_showOriginTerrainBoxes = EditorGUILayout.Toggle("Show Terrain Boxes", SessionManager.m_showOriginTerrainBoxes);
                        }

                        EditorGUILayout.EndHorizontal();

                    }
                    //else
                    //{
                    //    GUILayout.Space(Camera.current.pixelRect.size.x / 2f + 188);
                    //}
                }
                EditorGUILayout.EndVertical();
                GUIContent buttonContent = null;

                if (EditorGUIUtility.isProSkin)
                {
                    if (SessionManager.m_showSceneViewPanel)
                    {
                        buttonContent = new GUIContent(GaiaSettings.m_originUIProUnfoldUp, "Hide Gaia Panel");
                    }
                    else
                    {
                        buttonContent = new GUIContent(GaiaSettings.m_originUIProUnfoldDown, "Show Gaia Panel");
                    }

                }
                else
                {
                    if (SessionManager.m_showSceneViewPanel)
                    {
                        buttonContent = new GUIContent(GaiaSettings.m_originUIUnfoldUp, "Hide Gaia Panel");
                    }
                    else
                    {
                        buttonContent = new GUIContent(GaiaSettings.m_originUIUnfoldDown, "Show Gaia Panel");
                    }
                }
                if (GUILayout.Button(new GUIContent(buttonContent), GUIStylePanel, GUILayout.Width(20), GUILayout.Height(20)))
                {
                    SessionManager.m_showSceneViewPanel = !SessionManager.m_showSceneViewPanel;
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
#else               
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
#endif
            GUILayout.EndArea();

        }
    }
}
#endif
