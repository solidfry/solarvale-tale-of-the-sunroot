using UnityEngine;
using System.Collections;

public class FPSCounter : MonoBehaviour
{
    public bool SetTargetFrameRate = true;
    float deltaTime = 0.0f;

    private void Start()
    {
#if UNITY_ANDROID || UNITY_IPHONE
        if (SetTargetFrameRate)
        {
            Application.targetFrameRate = 60;
        }
#endif
    }

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(1, 1, 1, 1.0f);
        float fps = 1.0f / deltaTime;
        GUI.Label(rect, Mathf.RoundToInt(fps)+" fps", style);
    }
}
