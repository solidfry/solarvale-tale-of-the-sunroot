using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class WorldConnectionCredentials
{
    public string m_displayname = "New World";
    public string m_description = "World Description";
    public Texture2D m_previewImageTexture;
    public string m_catalogueURL = "http://example.com/SomeFolder/MyCatalogueFile.json";
    public string m_startSceneAddress = "Assets/SceneFolder/MyStartScene.unity";
}

/// <summary>
/// Holds a list of world connection credentials to load different worlds. To load a world, the URL address of the catalogue and the addressable name of the first scene to load is needed.
/// </summary>

[CreateAssetMenu(menuName = "Procedural Worlds/World Selection Configuration")]
[System.Serializable]
public class WorldSelectionConfiguration : ScriptableObject
{
    public List<WorldConnectionCredentials> m_worldConnectionCredentials = new List<WorldConnectionCredentials>();
}
