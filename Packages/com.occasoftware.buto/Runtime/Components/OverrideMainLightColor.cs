using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OccaSoftware.Buto.Runtime
{
    [AddComponentMenu("OccaSoftware/Buto/Override Main Light Color")]
    [ExecuteAlways]
    public class OverrideMainLightColor : MonoBehaviour
    {
        [ColorUsage(false, true)]
        public Color color;

        private void OnEnable()
        {
            Shader.SetGlobalInteger("buto_IsMainLightColorOverrideEnabled", 1);
        }

        private void Update()
        {
            Shader.SetGlobalVector("buto_MainLightColorOverride", color);
        }

        private void OnDisable()
        {
            Shader.SetGlobalInteger("buto_IsMainLightColorOverrideEnabled", 0);
        }
    }
}
