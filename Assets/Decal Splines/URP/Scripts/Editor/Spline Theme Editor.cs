using DecalSplines;
using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(SplineTheme))]
public class SplineThemeEditor : Editor
{
    public EventHandler OnActiveStyleChanged;

    private ReorderableList styleSelectionList;
    private const int objectPickerID = 992123552;
    private SerializedProperty styles;
    private SerializedProperty styleIndex;
    private NoneSplineStyle noSelection;

    public override void OnInspectorGUI()
    {
        Initialize();
        serializedObject.Update();

        AddNoSelectionStyle();

        //Style box
        EditorGUILayout.BeginVertical("Box");
        
        styleSelectionList.DoLayoutList();
        EditorGUILayout.EndVertical();

        // Add Style
        if (Event.current.commandName == "ObjectSelectorClosed" &&
                EditorGUIUtility.GetObjectPickerControlID() == objectPickerID)
        {
            ISplineStyle pickedStyle = (ISplineStyle)EditorGUIUtility.GetObjectPickerObject();
            AddStyle(pickedStyle);
        }

        // Add and Remove buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Style", GUILayout.Height(20)))
        {
            EditorGUIUtility.ShowObjectPicker<ISplineStyle>(null, false, "", objectPickerID);
        }
        if (GUILayout.Button("Remove Style", GUILayout.Height(20)))
        {
            RemoveStyle();
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void Initialize()
    {
        if (styles == null)
            styles = serializedObject.FindProperty("styles");
        if (styleIndex == null)
            styleIndex = serializedObject.FindProperty("styleIndex");

        if (noSelection == null)
            noSelection = NoneSplineStyle.FindAsset();

        if (styleSelectionList == null)
        {
            styleSelectionList = new ReorderableList(serializedObject, styles, true, false, false, false);
            styleSelectionList.index = StyleIndex;
            styleSelectionList.elementHeight = elementHeight;
            styleSelectionList.onSelectCallback = OnSelectStyle;
            styleSelectionList.drawElementCallback = DrawStyleElement;
            styleSelectionList.onReorderCallbackWithDetails = OnReorderStyleElement;

        }
    }

    private int StyleIndex
    {
        get
        {
            if (styleSelectionList != null)
            {
                if (styleIndex.intValue >= styles.arraySize)
                    styleIndex.intValue = 0;
            }
            return styleIndex.intValue;
        }
        set
        {
            if (styleSelectionList != null)
            {
                if (value < styles.arraySize)
                {
                    styleIndex.intValue = value;
                    OnActiveStyleChanged?.Invoke(this,EventArgs.Empty);
                }
            }
        }
    }

    private void AddNoSelectionStyle()
    {
        if (styles.arraySize == 0)
        {
            styles.arraySize += 1;
            styles.GetArrayElementAtIndex(0).objectReferenceValue = noSelection;
        }
        else if (styles.GetArrayElementAtIndex(0).objectReferenceValue != noSelection)
        {
            styles.InsertArrayElementAtIndex(0);
            styles.GetArrayElementAtIndex(0).objectReferenceValue = noSelection;
        }

    }

    private void AddStyle(ISplineStyle pickedStyle)
    {
        if (pickedStyle != null)
        {
            styles.arraySize += 1;
            SerializedProperty element = styles.GetArrayElementAtIndex(styles.arraySize - 1);
            element.objectReferenceValue = pickedStyle;
        }
    }

    private void RemoveStyle()
    {
        if (styleSelectionList.index > 0)
        {
            styles.DeleteArrayElementAtIndex(styleSelectionList.index);

            //update index if out of bounds
            if (styleSelectionList.index >= styles.arraySize - 1)
            {
                styleSelectionList.index = styles.arraySize - 1;
                StyleIndex = styleSelectionList.index;
            }
        }
    }

    private void OnReorderStyleElement(ReorderableList list, int oldIndex, int newIndex)
    {
        if (newIndex == 0 || oldIndex == 0)
            styles.MoveArrayElement(newIndex, oldIndex);
    }

    private void OnSelectStyle(ReorderableList list)
    {
        StyleIndex = list.index;
    }

    // Style Box sizes
    const int elementHeight = 70;
    const int objectFieldHeight = 16;
    const int elementPadding = 2;
    const int objectFieldWidth = 240;
    const int iconWidth = 64;
    const int iconHeight = 64;

    private void DrawStyleElement(Rect rect, int index, bool selected, bool focused)
    {
        rect.y = rect.y + elementPadding;
        Rect rectIcon = new Rect((rect.x), rect.y, iconWidth, iconHeight);
        Rect rectObjectField = new Rect((rectIcon.x + iconWidth + 10), rect.y, objectFieldWidth, objectFieldHeight);
        if (styles.arraySize > 0)
        {
            ISplineStyle style = styles.GetArrayElementAtIndex(index).objectReferenceValue as ISplineStyle;
            if (index == 0)
            {
                EditorGUILayout.BeginHorizontal();
                Texture2D icon = null;
                //Get material preview icon
                if (style.GetType() == typeof(DecalSplineStyle))
                {
                    DecalSplineStyle decalStyle = (DecalSplineStyle)style;
                    icon = AssetPreview.GetAssetPreview(decalStyle.Material);
                }
                GUI.Box(rectIcon, icon);
                GUI.Label(rectObjectField, "None");
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                Texture2D icon = null;

                //Get material preview icon
                if (style != null)
                {
                    if (style.GetType() == typeof(DecalSplineStyle))
                    {
                        DecalSplineStyle decalStyle = (DecalSplineStyle)style;
                        icon = AssetPreview.GetAssetPreview(decalStyle.Material);
                    }

                    //Get mesh style preview icon
                    if (style.GetType() == typeof(MeshSplineStyle))
                    {
                        MeshSplineStyle meshStyle = (MeshSplineStyle)style;
                        icon = AssetPreview.GetAssetPreview(meshStyle.Prefab.gameObject);
                    }
                }

                GUI.Box(rectIcon, icon);
                styles.GetArrayElementAtIndex(index).objectReferenceValue = EditorGUI.ObjectField(rectObjectField, styles.GetArrayElementAtIndex(index).objectReferenceValue, typeof(ISplineStyle), false) as ISplineStyle;
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
