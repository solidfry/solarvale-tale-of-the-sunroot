using UnityEngine;

public class UniStormStereoRenderer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (!UnityEngine.XR.XRSettings.enabled)
            return;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material.EnableKeyword("STEREO_INSTANCING_ON");
        }
    }
}
