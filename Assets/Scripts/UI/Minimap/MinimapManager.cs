using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private Transform mainCamera;

    private void LateUpdate()
    {
        transform.position = new Vector3(playerCharacter.transform.position.x, 400, playerCharacter.transform.position.z);

        Vector3 rotation = new Vector3(90, mainCamera.eulerAngles.y, 0);
        transform.rotation = Quaternion.Euler(rotation);
    }

}
