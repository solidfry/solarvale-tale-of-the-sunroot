using UnityEngine;

public class BillboardLineRendererCircle : MonoBehaviour {
    public Color color = Color.black;
    public float width = 1f;
    public int numSegments = 50;
    public float radius = 0.5f;

    private LineRenderer _lineRenderer;

    void Start() {
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (_lineRenderer != null) return;

        // Initialize line renderer.
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.materials = new[] {
            new Material(Shader.Find("Universal Render Pipeline/Unlit")) { color = color }
        };
        _lineRenderer.startWidth = width * 0.01f;
        _lineRenderer.endWidth = width * 0.01f;
        _lineRenderer.positionCount = numSegments + 1;
        _lineRenderer.useWorldSpace = false;

        // Create points.
        float deltaTheta = (float)(2.0 * Mathf.PI) / numSegments;
        float theta = 0f;

        for (int i = 0; i < numSegments + 1; i++) {
            float x = Mathf.Cos(theta);
            float y = Mathf.Sin(theta);
            Vector3 pos = new Vector3(x, y, 0);
            _lineRenderer.SetPosition(i, pos * radius);
            theta += deltaTheta;
        }
    }

    [ContextMenu("Reinitialize")]
    private void Reinitialize() {
        if (_lineRenderer != null) {
            DestroyImmediate(_lineRenderer);
        }

        Start();
        Update();
    }

    private void Update() {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);
    }
}