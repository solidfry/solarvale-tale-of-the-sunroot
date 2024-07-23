using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cam = UnityEngine.Camera;

namespace UI.Minimap
{
    public class QuestIconManager : MonoBehaviour
    {
        [SerializeField] private GameObject questIconPrefab;
        [SerializeField] private GameObject playerCharacter;
        [SerializeField] private Cam minimapCamera;
        [SerializeField] private MiniMapController miniMap;

        private List<(ObjectivePosition objectivePosition, RectTransform markerRectTransform)> currentObjectives;
        private bool _miniMapInitialized = false;

        private void Awake()
        {
            currentObjectives = new List<(ObjectivePosition objectivePosition, RectTransform markerRectTransform)>();
        }

        private void Start()
        {
            InitialiseMiniMap();
        }

        private void Update()
        {
            if (miniMap == null)
            {
                miniMap = FindObjectOfType<MiniMapController>();
                InitialiseMiniMap();
            }

            RectTransform mapBorderRectTransform = GetMapObjectRectTransform(0);
            if (mapBorderRectTransform != null)
            {
                foreach ((ObjectivePosition objectivePosition, RectTransform markerRectTransform) marker in currentObjectives)
                {
                    Vector3 offset = marker.objectivePosition.transform.position - playerCharacter.transform.position;
                    offset /= minimapCamera.orthographicSize;
                    offset *= mapBorderRectTransform.rect.width / 2f;
                    offset.x = Mathf.Clamp(offset.x, -mapBorderRectTransform.rect.width / 2f, mapBorderRectTransform.rect.width / 2f);
                    offset.z = Mathf.Clamp(offset.z, -mapBorderRectTransform.rect.width / 2f, mapBorderRectTransform.rect.width / 2f);
                    marker.markerRectTransform.anchoredPosition = new Vector2(offset.x, offset.z);
                }
            }
        }

        private void InitialiseMiniMap()
        {
            if (miniMap != null && !_miniMapInitialized)
            {
                miniMap.Initialise();
                _miniMapInitialized = true;
                miniMap.gameObject.SetActive(true);  // Ensure the minimap is active
            }
        }

        public void AddObjectiveMarker(ObjectivePosition sender)
        {
            RectTransform mapBorderRectTransform = GetMapObjectRectTransform(0);
            if (mapBorderRectTransform != null)
            {
                RectTransform rectTransform = Instantiate(questIconPrefab, mapBorderRectTransform).GetComponent<RectTransform>();
                currentObjectives.Add((sender, rectTransform));
            }
        }

        public void RemoveObjectiveMarker(ObjectivePosition sender)
        {
            if (currentObjectives.Exists(objective => objective.objectivePosition == sender))
            {
                (ObjectivePosition pos, RectTransform rectTrans) foundObj = currentObjectives.Find(objective => objective.objectivePosition == sender);
                Destroy(foundObj.rectTrans.gameObject);
                currentObjectives.Remove(foundObj);
            }
        }

        private RectTransform GetMapObjectRectTransform(int index)
        {
            if (miniMap != null)
            {
                GameObject mapObject = miniMap.GetMapObject(index);
                if (mapObject != null)
                {
                    return mapObject.GetComponent<RectTransform>();
                }
            }
            return null;
        }
    }
}
