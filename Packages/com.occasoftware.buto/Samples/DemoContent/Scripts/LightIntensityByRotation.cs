using UnityEngine;

namespace OccaSoftware.Buto.Demo
{
    [ExecuteAlways]
    [AddComponentMenu("Buto/Demo/Set Light Intensity by Rotation")]
    public class LightIntensityByRotation : MonoBehaviour
    {
        Light l;

        [SerializeField]
        float intensity = 1.0f;

        // Update is called once per frame
        void Update()
        {
            if (l == null)
                l = GetComponent<Light>();

            float up = Mathf.Clamp01(Vector3.Dot(Vector3.Normalize(-transform.forward), Vector3.up));
            up = Mathf.Pow(up, 0.5f);
            l.intensity = Mathf.Lerp(0.001f, intensity, up);
        }
    }
}
