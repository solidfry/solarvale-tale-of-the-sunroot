using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DollyLookAtManager : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Assign the virtual camera in the Inspector
    public CinemachineDollyCart dollyCart; // Assign the dolly cart in the Inspector
    public GameObject[] lookAtTargets; // Array of GameObjects to look at
    private int currentTargetIndex = 0; // Index for current target
    private float currentTime = 0f; 
    private float switchTime0 = 16f; //Look at Sunroot
    private float switchTime1 = 32f; // Look at Cave Painting and set speed to 200
    private float switchTime2 = 33.72f; // Set speed to 5 
    private float switchTime3 = 42.3f; // Look at Whale Rock and set speed to 200
    private float switchTime4 = 45.1f; // Set speed to 5 
    private float switchTime5 = 58f; // Look at home 
    private float switchTime6 = 59f; // Set speed to 5

    [SerializeField] private Animator outroText1;
    [SerializeField] private Animator outroText2;
    [SerializeField] private Animator outroText3;
    [SerializeField] private Animator outroText4;
    [SerializeField] private Animator outroText5;


    void Update()
    {
        currentTime += Time.deltaTime;
        if(currentTime < switchTime0)
        {
            //Look at Grandma
            currentTargetIndex = 0;
            virtualCamera.LookAt = lookAtTargets[currentTargetIndex].transform;

            //Fade in outro 1
            outroText1.SetBool("Fade", true);
        }

        if (currentTime >= switchTime0 && currentTime < switchTime1)
        {
            //Look at Sunroot
            currentTargetIndex = 1;
            virtualCamera.LookAt = lookAtTargets[currentTargetIndex].transform;

            outroText2.SetBool("Fade", true);
        }

        if (currentTime >= switchTime1 && currentTime < switchTime2)
        {
            //Look at Cave painting and set speed to 200
            currentTargetIndex = 2;
            virtualCamera.LookAt = lookAtTargets[currentTargetIndex].transform;
            dollyCart.m_Speed = 200;
        }

        if (currentTime >= switchTime2 && currentTime < switchTime3)
        {
            // Set speed to 5 
            dollyCart.m_Speed = 4;
            outroText3.SetBool("Fade", true);
        }

        if (currentTime >= switchTime3 && currentTime < switchTime4)
        {
            // Look at Whale Rock and set speed to 200
            currentTargetIndex = 3;
            virtualCamera.LookAt = lookAtTargets[currentTargetIndex].transform;
            dollyCart.m_Speed = 200;
            Debug.Log("Look at Whale Rock and set speed to 200");
        }

        if (currentTime >= switchTime4 && currentTime < switchTime5)
        {
            // Set speed to 5 
            dollyCart.m_Speed = 20;
            outroText4.SetBool("Fade", true);
        }

        if (currentTime >= switchTime5 && currentTime < switchTime6)
        {
            // Look at home and set speed to 200
            currentTargetIndex = 4;
            virtualCamera.LookAt = lookAtTargets[currentTargetIndex].transform;
            dollyCart.m_Speed = 200;
            Debug.Log("Look at home and set speed to 200");
        }

        if (currentTime >= switchTime6)
        {
            // Set speed to 5
            dollyCart.m_Speed = 5;
            outroText5.SetBool("Fade", true);
        }
    }
}
