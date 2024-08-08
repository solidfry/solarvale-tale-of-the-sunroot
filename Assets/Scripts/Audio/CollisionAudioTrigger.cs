using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionAudioTrigger : MonoBehaviour
{
    private AkAmbient akAmbient;
    private float fadeOutDuration = 2.0f;
    [SerializeField] private string wwiseEventName;
    [SerializeField] private float minTimeWait;
    [SerializeField] private float maxTimeWait;

    private Coroutine playSoundCoroutine;

    private void Start()
    {
        akAmbient = GetComponent<AkAmbient>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Start the coroutine to play sound at random intervals between 0.75 and 2 seconds
            playSoundCoroutine = StartCoroutine(PlaySoundAtIntervals());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Stop the coroutine to cease playing sounds
            if (playSoundCoroutine != null)
            {
                StopCoroutine(playSoundCoroutine);
                playSoundCoroutine = null;
            }

            // Fade out the currently playing sound
            AkSoundEngine.ExecuteActionOnEvent(wwiseEventName, AkActionOnEventType.AkActionOnEventType_Stop, gameObject, (int)(fadeOutDuration * 1000), AkCurveInterpolation.AkCurveInterpolation_Linear);
        }
    }

    private IEnumerator PlaySoundAtIntervals()
    {
        while (true) // Loop continuously until the coroutine is stopped
        {
            // Play the ambient sound
            akAmbient.HandleEvent(gameObject);

            // Wait for a random time between 0.75 and 2 seconds
            float waitTime = Random.Range(minTimeWait, maxTimeWait);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
