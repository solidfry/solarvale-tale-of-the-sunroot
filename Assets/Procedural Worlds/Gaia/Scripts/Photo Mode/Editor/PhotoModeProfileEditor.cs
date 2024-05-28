using UnityEngine;
using UnityEditor;

namespace Gaia
{
    [CustomEditor(typeof(PhotoModeProfile))]
    public class PhotoModeProfileEditor : Editor
    {
        private PhotoModeProfile m_profile;

        public override void OnInspectorGUI()
        {
            m_profile = (PhotoModeProfile) target;

            if (GUILayout.Button(new GUIContent("Reset", "Resets the profile back to default state.")))
            {
                m_profile.Reset();
                EditorUtility.SetDirty((m_profile));
            }
        }
    }
}