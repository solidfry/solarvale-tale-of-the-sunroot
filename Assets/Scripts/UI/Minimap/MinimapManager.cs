using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapManager : MonoBehaviour
{
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private Transform mainCamera;

    [SerializeField] private GameObject miniMap;

    [SerializeField] private Animator miniMapBoarderAnimator;
    [SerializeField] private Animator miniMapImageAnimator;


    private void Start()
    {
        miniMap.SetActive(false);
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(playerCharacter.transform.position.x, 400, playerCharacter.transform.position.z);

        Vector3 rotation = new Vector3(90, mainCamera.eulerAngles.y, 0);
        transform.rotation = Quaternion.Euler(rotation);
    }

    public void UnlockMiniMap()
    {
        miniMap.SetActive(true);
        miniMapBoarderAnimator.SetBool("MinimapUnlocked", true);
        miniMapImageAnimator.SetBool("MinimapUnlocked", true);

    }

}
