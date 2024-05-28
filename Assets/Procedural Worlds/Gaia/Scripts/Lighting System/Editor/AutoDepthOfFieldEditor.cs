using UnityEditor;

namespace Gaia
{
    [CustomEditor(typeof(AutoDepthOfField))]
    public class AutoDepthOfFieldEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This is the auto depth of field system for Gaia. To edit or remove please go to Gaia Player under Gaia Runtime", MessageType.Info);
        }
    }
}