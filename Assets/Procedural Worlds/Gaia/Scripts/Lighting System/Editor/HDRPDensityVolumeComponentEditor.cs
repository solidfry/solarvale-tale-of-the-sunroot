#if HDPipeline
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Gaia
{
    [CustomEditor(typeof(HDRPDensityVolumeComponent))]
    public class HDRPDensityVolumeComponentEditor : Editor
    {
        private HDRPDensityVolumeComponent m_component;

        private void OnEnable()
        {
            m_component = (HDRPDensityVolumeComponent) target;
            if (m_component != null)
            {
                m_component.Setup();
            }
        }

        public override void OnInspectorGUI()
        {
            if (m_component == null)
            {
                m_component = (HDRPDensityVolumeComponent) target;
            }

            EditorGUILayout.HelpBox("The HDRP Density Volume Component is an extension component you can add to other density volumes in your scene. " +
                                    "This system adds a box collider based on the density volume bounds, these bounds are used to check to see if you are within the volume. " +
                                    "If you are in the volume it will disable the global density volume using this volume. To use this system you must be using the HDRP Density Volume Controller", MessageType.Info);

            m_component.m_isVolumeActive = EditorGUILayout.Toggle(new GUIContent("Is Volume Active", "You can set this to false if you don't want the density volume to process this density volume. This can be used when you prefer the global volume over the one placed in the scene"), m_component.m_isVolumeActive);
            if (!m_component.m_isVolumeActive)
            {
                GUI.enabled = false;
            }
#if UNITY_2021_2_OR_NEWER
            m_component.m_densityVolume = (LocalVolumetricFog)EditorGUILayout.ObjectField(new GUIContent("Density Volume", "The density volume that is used to build the bounds, HDRP Density Volume Component should be attached to the root level of the density volume to give accurate bounds."), m_component.m_densityVolume, typeof(LocalVolumetricFog), true);
#else
            m_component.m_densityVolume = (DensityVolume)EditorGUILayout.ObjectField(new GUIContent("Density Volume", "The density volume that is used to build the bounds, HDRP Density Volume Component should be attached to the root level of the density volume to give accurate bounds."), m_component.m_densityVolume, typeof(DensityVolume), true);
#endif
            GUI.enabled = true;
        }
    }
}
#endif