using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
#if PW_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
#endif
using UnityEngine.UI;
using UnityEngine.UIElements;

public class WorldSelectionUI : MonoBehaviour
{

    public WorldSelectionConfiguration m_worldSelectionConfiguration;
    public GameObject m_worldSelectionEntryPrefab;
    public Transform m_scrollViewContentTransform;
    #if PW_ADDRESSABLES
    string m_targetScene = "";
#endif

    public bool m_useCachedData = true;
    public UnityEngine.UI.Slider m_progressSlider;
    public Text m_sceneAName;
    public Text m_sceneA2Name;
    public Text m_sceneBName;
    public Text m_enteringText;
    public Text m_statusText;
    public Text m_welcomeText;
    public Text m_downloadProgressText;
    

    // Start is called before the first frame update
    void Start()
    {
        List<string> cachePaths = new List<string>();
#if !UNITY_WEBGL
        Caching.GetAllCachePaths(cachePaths);
#else
        Debug.LogError("The Addressable Demo Client is not suitable for use with WebGL!");
#endif
        foreach (string path in cachePaths)
        {
            Debug.Log("Cache path: " + path);
        }

        m_progressSlider.gameObject.SetActive(false);
        m_enteringText.text = "";
        m_statusText.text = "";
        m_downloadProgressText.text = "";
        m_welcomeText.enabled = true;

        if (m_worldSelectionConfiguration != null && m_worldSelectionEntryPrefab!=null && m_scrollViewContentTransform !=null)
        {
            //Clear out sample entries first
            foreach (Transform t in m_scrollViewContentTransform)
            {
                Destroy(t.gameObject);
            }
            foreach (WorldConnectionCredentials entry in m_worldSelectionConfiguration.m_worldConnectionCredentials)
            {
                GameObject newEntryGO = Instantiate(m_worldSelectionEntryPrefab, m_scrollViewContentTransform);
                WorldSelectionEntryUI wseUI = newEntryGO.GetComponent<WorldSelectionEntryUI>();
                if (wseUI != null)
                {
                    wseUI.m_worldSelectionUI = this;
                    wseUI.m_worldNameText.text = entry.m_displayname;
                    wseUI.m_worldDescriptionText.text = entry.m_description;
                    wseUI.m_catalogueURL = entry.m_catalogueURL;
                    wseUI.m_startSceneAddress = entry.m_startSceneAddress;
                    wseUI.m_previewImage.texture = entry.m_previewImageTexture;
                }
                else
                {
                    Debug.LogError("Created a world selection entry, but it looks like it is missing the world selection entry UI in the prefab?");
                }
            }
        }
        else
        {
            if (m_worldSelectionConfiguration == null)
            {
                Debug.LogError("No World Selection Configuration Scriptable Object assigned for the World Selection UI!");
            }

            if (m_worldSelectionEntryPrefab == null)
            {
                Debug.LogError("No UI Prefab for the World Selection entries assigned for the World Selection UI!");
            }

            if (m_scrollViewContentTransform == null)
            {
                Debug.LogError("No target scroll view content transform assigned in the World Selection UI!");
            }
        }

      

    }


    public void LoadWorld(string catalogueURL, string startSceneAddress, string worldName)
    {
#if PW_ADDRESSABLES
        m_enteringText.text = "Entering " + worldName;
        m_targetScene = startSceneAddress;
        m_statusText.text = "Checking for updates...";
        Addressables.LoadContentCatalogAsync(catalogueURL).Completed += OnCatalogCompleted;
#endif
    }

  
#if PW_ADDRESSABLES
    private void OnCatalogCompleted(AsyncOperationHandle<IResourceLocator> obj)
    {
        m_welcomeText.enabled = false;
        StartCoroutine(LoadAsynchronously());
      
    }


    public void ToggleChanged(bool newValue)
    {
            m_useCachedData = newValue;
    }

    private IEnumerator LoadAsynchronously()
    {
        if (!m_useCachedData)
        {
            Addressables.ClearDependencyCacheAsync(m_targetScene);
        }

        //Check the download size
        AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(m_targetScene);
        yield return getDownloadSize;

        //If the download size is greater than 0, download all the dependencies.
        if (getDownloadSize.Result > 0)
        {
            m_progressSlider.gameObject.SetActive(true);
            m_progressSlider.value = 0;
            float totalSize = getDownloadSize.Result / 1000000;
            
            AsyncOperationHandle asyncOperation = Addressables.DownloadDependenciesAsync(m_targetScene);
            while (!asyncOperation.IsDone)
            {
                float progressValue = Mathf.InverseLerp(0.875f, 0.987f, asyncOperation.PercentComplete); 
                if (asyncOperation.PercentComplete < 0.87)
                {
                    m_statusText.text = $"Update found, Total Size: {totalSize} MB. Preparing Download...";
                }
                else
                {
                    m_statusText.text = $"Downloading, Total Size: {totalSize} MB";
                    m_downloadProgressText.text = $"{progressValue * 100:0.00} %";
                }
                m_progressSlider.value = progressValue;
                yield return null;
            }
        }
        m_progressSlider.gameObject.SetActive(false);
        m_statusText.text = "Starting Scene...";
        m_downloadProgressText.text = "";
        Addressables.LoadSceneAsync(m_targetScene);
    }
#endif

}
