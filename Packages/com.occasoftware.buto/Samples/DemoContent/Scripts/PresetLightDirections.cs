using System.Collections.Generic;
using UnityEngine;

namespace OccaSoftware.Buto.Demo
{
    [AddComponentMenu("Buto/Demo/Preset Light Directions")]
    public class PresetLightDirections : MonoBehaviour
    {
        public List<Vector3> lightPositions;
        int index = 0;

        public bool storeCurrent = false;

        private void OnValidate()
        {
            if (storeCurrent)
            {
                lightPositions.Add(transform.rotation.eulerAngles);
                storeCurrent = false;
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                transform.rotation = Quaternion.Euler(lightPositions[index % lightPositions.Count]);
                index++;
            }
        }
    }
}
