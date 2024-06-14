using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    public Camera mainCamera;
    public Transform firstPersonCameraPosition;

    void Update()
    {
        mainCamera.transform.position = firstPersonCameraPosition.position;
        mainCamera.transform.rotation = firstPersonCameraPosition.rotation;
    }
}
