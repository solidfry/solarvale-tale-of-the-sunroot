using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public GameObject uiPanel;
    public Slider progressSlider;
    public TextMeshProUGUI progressText; // Add a Text UI element to display the percentage

    public void LoadScene(int index)
    {
        StartCoroutine(LoadScene_Coroutine(index));
    }

    private IEnumerator LoadScene_Coroutine(int index)
    {
        progressSlider.value = 0;
        uiPanel.SetActive(true);

        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(index);
        asyncOperation.allowSceneActivation = false;

        while (!asyncOperation.isDone)
        {
            // asyncOperation.progress is capped at 0.9f, so we multiply by 1/0.9f to get the correct percentage
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            progressSlider.value = progress;
            progressText.text = $"{(progress * 100):0}%"; // Update the percentage text

            // Wait until the asynchronous scene fully loads
            if (asyncOperation.progress >= 0.9f)
            {
                progressSlider.value = 1;
                progressText.text = "100%";
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
