using UnityEngine;
using Random = UnityEngine.Random;

namespace Dustyroom {
public class FloatingMotion : MonoBehaviour {
    public float verticalAmplitude = 1.0f;
    public float horizontalAmplitude = 0.0f;

    [Space]
    public float speed = 1.0f;

    [Space, Tooltip("In seconds")]
    public float startDelay = 0;

    [Space]
    public bool worldSpace = false;

    private Vector3 initialPosition;
    private float offsetH = 0f;
    private float offsetV = 0f;
    private bool isMoving = false;

    void Start() {
        Invoke("Initialize", startDelay);
    }

    private void Initialize() {
        initialPosition = worldSpace ? transform.position : transform.localPosition;
        offsetH = Random.value * 1000f;
        offsetV = Random.value * 1000f;
        isMoving = true;
    }

    void Update() {
        if (!isMoving) {
            return;
        }

        var hDirection = new Vector3(Mathf.Sin(Time.timeSinceLevelLoad * speed * 0.5f + offsetV + 100f), 0f,
                                     Mathf.Cos(Time.timeSinceLevelLoad * speed + offsetV + 100f));
        Vector3 offset = Vector3.up * Mathf.Sin(Time.timeSinceLevelLoad * speed + offsetH) * verticalAmplitude +
                         hDirection * Mathf.Sin(Time.timeSinceLevelLoad * speed + offsetV) * horizontalAmplitude;
        Vector3 position = initialPosition + offset * Time.timeScale;
        if (worldSpace) {
            transform.position = position;
        } else {
            transform.localPosition = position;
        }
    }
}
}