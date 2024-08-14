using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Events;

public class DollyLookAtManager : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera; // Assign the virtual camera in the Inspector
    public CinemachineDollyCart dollyCart; // Assign the dolly cart in the Inspector
    public GameObject[] lookAtTargets; // Array of GameObjects to look at
    private int currentTargetIndex = 0; // Index for current target

    [Header("TIME TO TRANSITION")]
    [SerializeField] private float currentTime = 0f;
    [SerializeField] private float switchTime0 = 10f; //Look at Sunroot
    [SerializeField] private float switchTime1 = 20f; // Look at Cave Painting and set speed to 200
    [SerializeField] private float switchTime2 = 30.72f; // Set speed to 5 
    [SerializeField] private float switchTime3 = 34.3f; // Look at Whale Rock and set speed to 200
    [SerializeField] private float switchTime4 = 37.1f; // Set speed to 5 
    [SerializeField] private float switchTime5 = 50f; // Look at home 
    [SerializeField] private float switchTime6 = 51.5f; // Set speed to 5
    [SerializeField] private float switchTime7 = 58f; // Set Blackscreen
    [SerializeField] private float switchTime8 = 62f; // Load new Scene

    [Header("OUTRO TEXT")]
    [SerializeField] private Animator blackScreen;
    [SerializeField] private Animator outroText1;
    [SerializeField] private Animator outroText2;
    [SerializeField] private Animator outroText3;
    [SerializeField] private Animator outroText4;
    [SerializeField] private Animator outroText5;

    private int currentCartSpeed ;
    private int maxCartSpeed = 200;
    private int minCartSpeed = 5;

    public UnityEvent loadGameOverScene;
    public UnityEvent pianoKeySound;

    private bool hasPlayedOutro1 = false;
    private bool hasPlayedOutro2 = false;
    private bool hasPlayedOutro3 = false;
    private bool hasPlayedOutro4 = false;
    private bool hasPlayedOutro5 = false;

    private void Start()
    {
        blackScreen.SetBool("Fade", true);
        currentCartSpeed = minCartSpeed;
    }
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
            if (!hasPlayedOutro1)
            {
                playPianoKeysOnce();
                hasPlayedOutro1 = true;
            }
        }

        if (currentTime >= switchTime0 && currentTime < switchTime1)
        {
            //Look at Sunroot
            currentTargetIndex = 1;
            virtualCamera.LookAt = lookAtTargets[currentTargetIndex].transform;

            outroText2.SetBool("Fade", true);
            if (!hasPlayedOutro2)
            {
                playPianoKeysOnce();
                hasPlayedOutro2 = true;
            }
        }

        if (currentTime >= switchTime1 && currentTime < switchTime2)
        {
            //Look at Cave painting and set speed to 200
            currentTargetIndex = 2;
            virtualCamera.LookAt = lookAtTargets[currentTargetIndex].transform;
            if(currentCartSpeed <= maxCartSpeed)
            {
                currentCartSpeed++;
            }
            else
            {
                currentCartSpeed = maxCartSpeed;
            }
            dollyCart.m_Speed = currentCartSpeed;
        }

        if (currentTime >= switchTime2 && currentTime < switchTime3)
        {
            // Set speed to 5 
            if (currentCartSpeed >= minCartSpeed)
            {
                currentCartSpeed--;
            }
            else
            {
                currentCartSpeed = minCartSpeed;
            }
            dollyCart.m_Speed = currentCartSpeed;
            outroText3.SetBool("Fade", true);
            if (!hasPlayedOutro3)
            {
                playPianoKeysOnce();
                hasPlayedOutro3 = true;
            }
        }

        if (currentTime >= switchTime3 && currentTime < switchTime4)
        {
            // Look at Whale Rock and set speed to 200
            currentTargetIndex = 3;
            virtualCamera.LookAt = lookAtTargets[currentTargetIndex].transform;
            if (currentCartSpeed <= maxCartSpeed)
            {
                currentCartSpeed++;
            }
            else
            {
                currentCartSpeed = maxCartSpeed;
            }
            dollyCart.m_Speed = currentCartSpeed;
        }

        if (currentTime >= switchTime4 && currentTime < switchTime5)
        {
            // Set speed to 5 
            if (currentCartSpeed >= 20)
            {
                currentCartSpeed--;
            }
            else
            {
                currentCartSpeed = 20;
            }
            dollyCart.m_Speed = currentCartSpeed;
            outroText4.SetBool("Fade", true);
            if (!hasPlayedOutro4)
            {
                playPianoKeysOnce();
                hasPlayedOutro4 = true;
            }
        }

        if (currentTime >= switchTime5 && currentTime < switchTime6)
        {
            // Look at home and set speed to 200
            currentTargetIndex = 4;
            virtualCamera.LookAt = lookAtTargets[currentTargetIndex].transform;
            if (currentCartSpeed <= maxCartSpeed)
            {
                currentCartSpeed++;
            }
            else
            {
                currentCartSpeed = maxCartSpeed;
            }
            dollyCart.m_Speed = currentCartSpeed;
        }

        if (currentTime >= switchTime6 && currentTime < switchTime7)
        {
            // Set speed to 5
            if (currentCartSpeed >= minCartSpeed)
            {
                currentCartSpeed--;
            }
            else
            {
                currentCartSpeed = minCartSpeed;
            }
            dollyCart.m_Speed = currentCartSpeed;
            outroText5.SetBool("Fade", true);
            if (!hasPlayedOutro5)
            {
                playPianoKeysOnce();
                hasPlayedOutro5 = true;
            }

        }
        if (currentTime >= switchTime7 && currentTime < switchTime8)
        {
            // Set Blackscreen
            blackScreen.SetBool("Fade", false);
        }

        if (currentTime >= switchTime8)
        {
            loadGameOverScene.Invoke();
        }
    }
    public void playPianoKeysOnce()
    {
        pianoKeySound.Invoke();
    }
}
