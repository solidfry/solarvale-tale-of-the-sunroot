using System.Collections;
using System.Collections.Generic;
using Minimap;
using UnityEngine;

public enum MinimapMode
{
    Mini, Fullscreen
}

public class MinimapController : MonoBehaviour
{
    public static MinimapController Instance;

    [SerializeField] Vector2 worldSize;
    [SerializeField] Vector2 fullScreenDimensions = new Vector2(1000, 1000);
    [SerializeField] float zoomSpeed = 0.1f;
    [SerializeField] float maxZoom = 10f;
    [SerializeField] float minZoom = 1f;
    [SerializeField] RectTransform scrollViewRectTransform;
    [SerializeField] public RectTransform contentRectTransform;
    [SerializeField] MinimapIcon minimapIconPrefab;

    Matrix4x4 transformationMatrix;

    private MinimapMode currentMiniMapMode = MinimapMode.Mini;
    private MinimapIcon followIcon;
    private Vector2 scrollViewDefaultSize;
    private Vector2 scrollViewDefaultPosition;
    Dictionary<MinimapWorldObject, MinimapIcon> miniMapWorldObjectsLookup = new Dictionary<MinimapWorldObject, MinimapIcon>();

    [SerializeField] GameObject[] mapObjects;
    public void Initialise()
    {
        foreach (var mapObject in mapObjects)
        {
            mapObject.SetActive(true);
        }
    }
    public GameObject GetMapObject(int index)
    {
        if (index >= 0 && index < mapObjects.Length)
        {
            return mapObjects[index];
        }
        return null;
    }
    private void Awake()
    {
        Instance = this;
        scrollViewDefaultSize = scrollViewRectTransform.sizeDelta;
        scrollViewDefaultPosition = scrollViewRectTransform.anchoredPosition;
    }

    private void Start()
    {
        CalculateTransformationMatrix();
    }

    private void Update()
    {
        float zoom = Input.GetAxis("Mouse ScrollWheel");
        ZoomMap(zoom);
        UpdateMiniMapIcons();
        CenterMapOnIcon();
    }

    public void RegisterMinimapWorldObject(MinimapWorldObject miniMapWorldObject, bool followObject = false)
    {
        var minimapIcon = Instantiate(minimapIconPrefab, contentRectTransform);
        minimapIcon.Image.sprite = miniMapWorldObject.MinimapIcon;
        miniMapWorldObjectsLookup[miniMapWorldObject] = minimapIcon;

        // Set the initial position of the icon on the minimap
        minimapIcon.RectTransform.anchoredPosition = WorldPositionToMapPosition(miniMapWorldObject.transform.position);

        if (followObject)
            followIcon = minimapIcon;
    }

    public void RemoveMinimapWorldObject(MinimapWorldObject minimapWorldObject)
    {
        if (miniMapWorldObjectsLookup.TryGetValue(minimapWorldObject, out MinimapIcon icon))
        {
            miniMapWorldObjectsLookup.Remove(minimapWorldObject);
            // if (icon == followIcon)
            //     followIcon = null;
            if (icon != null)
                Destroy(icon.gameObject);
        }
    }

    private Vector2 halfVector2 = new Vector2(0.5f, 0.5f);

    public void SetMinimapMode(MinimapMode mode)
    {
        const float defaultScaleWhenFullScreen = 1.3f; // 1.3f looks good here but it could be anything

        if (mode == currentMiniMapMode)
            return;

        switch (mode)
        {
            case MinimapMode.Mini:
                scrollViewRectTransform.sizeDelta = scrollViewDefaultSize;
                scrollViewRectTransform.anchorMin = Vector2.one;
                scrollViewRectTransform.anchorMax = Vector2.one;
                scrollViewRectTransform.pivot = Vector2.one;
                scrollViewRectTransform.anchoredPosition = scrollViewDefaultPosition;
                currentMiniMapMode = MinimapMode.Mini;
                break;
            case MinimapMode.Fullscreen:
                scrollViewRectTransform.sizeDelta = fullScreenDimensions;
                scrollViewRectTransform.anchorMin = halfVector2;
                scrollViewRectTransform.anchorMax = halfVector2;
                scrollViewRectTransform.pivot = halfVector2;
                scrollViewRectTransform.anchoredPosition = Vector2.zero;
                currentMiniMapMode = MinimapMode.Fullscreen;
                contentRectTransform.transform.localScale = Vector3.one * defaultScaleWhenFullScreen;
                break;
        }
    }

    private void ZoomMap(float zoom)
    {
        if (zoom == 0)
            return;

        float currentMapScale = contentRectTransform.localScale.x;
        // we need to scale the zoom speed by the current map scale to keep the zooming linear
        float zoomAmount = (zoom > 0 ? zoomSpeed : -zoomSpeed) * currentMapScale;
        float newScale = currentMapScale + zoomAmount;
        float clampedScale = Mathf.Clamp(newScale, minZoom, maxZoom);
        contentRectTransform.localScale = Vector3.one * clampedScale;
    }

    private void CenterMapOnIcon()
    {
        if (followIcon != null)
        {
            float mapScale = contentRectTransform.transform.localScale.x;
            // we simply move the map in the opposite direction the player moved, scaled by the mapscale
            contentRectTransform.anchoredPosition = (-followIcon.RectTransform.anchoredPosition * mapScale);
        }
    }

    private void UpdateMiniMapIcons()
    {
        // scale icons by the inverse of the mapscale to keep them a consistent size
        float iconScale = 1 / contentRectTransform.transform.localScale.x;
        foreach (var kvp in miniMapWorldObjectsLookup)
        {
            var miniMapWorldObject = kvp.Key;
            var miniMapIcon = kvp.Value;
            var mapPosition = WorldPositionToMapPosition(miniMapWorldObject.transform.position);

            miniMapIcon.RectTransform.anchoredPosition = mapPosition;
            var rotation = miniMapWorldObject.transform.rotation.eulerAngles;
            miniMapIcon.IconRectTransform.localRotation = Quaternion.AngleAxis(-rotation.y, Vector3.forward);
            miniMapIcon.IconRectTransform.localScale = Vector3.one * iconScale;
        }
    }

    public Vector2 WorldPositionToMapPosition(Vector3 worldPos)
    {
        // Adjust worldPos to account for the offset
        Vector2 adjustedPos = new Vector2(worldPos.x + 512f, worldPos.z + 512f);
        return (Vector2)transformationMatrix.MultiplyPoint3x4(adjustedPos);
    }

    private void CalculateTransformationMatrix()
    {
        Vector2 minimapSize = contentRectTransform.rect.size; // Minimap dimensions (1000x1000)
        Vector2 worldSize = this.worldSize; // World dimensions (1024x1024)

        Vector2 scaleRatio = minimapSize / worldSize;
        Vector2 translation = -minimapSize / 2f;

        transformationMatrix = Matrix4x4.TRS(translation, Quaternion.identity, new Vector3(scaleRatio.x, scaleRatio.y, 1f));
    }
}
