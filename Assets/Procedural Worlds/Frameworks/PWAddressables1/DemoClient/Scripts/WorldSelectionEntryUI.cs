using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldSelectionEntryUI : MonoBehaviour
{
    public WorldSelectionUI m_worldSelectionUI;
    public Text m_worldNameText;
    public Text m_worldDescriptionText;
    public RawImage m_previewImage;
    public string m_catalogueURL;
    public string m_startSceneAddress;

    public void OnButtonClicked()
    {
        if (m_worldSelectionUI != null)
        {
            m_worldSelectionUI.LoadWorld(m_catalogueURL, m_startSceneAddress, m_worldNameText.text);
        }
    }

}
