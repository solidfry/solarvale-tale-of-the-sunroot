using System.Collections.Generic;
using UnityEngine;

namespace UI.Minimap
{
    public class QuestIconManager : MonoBehaviour
    {
        [SerializeField] private GameObject questIconPrefab;
        [SerializeField] private GameObject questAreaIconPrefab;
        [SerializeField] private GameObject playerCharacter;
        [SerializeField] private MinimapController miniMap; // Updated to use MinimapController

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
                miniMap = MinimapController.Instance;
                InitialiseMiniMap();
            }

            if (miniMap != null)
            {
                RectTransform contentRectTransform = miniMap.contentRectTransform; // Use property or method
                if (contentRectTransform != null)
                {
                    foreach ((ObjectivePosition objectivePosition, RectTransform markerRectTransform) marker in currentObjectives)
                    {
                        Vector2 minimapPosition = miniMap.WorldPositionToMapPosition(marker.objectivePosition.transform.position);
                        marker.markerRectTransform.anchoredPosition = minimapPosition;
                    }
                }
            }
        }

        private void InitialiseMiniMap()
        {
            if (miniMap != null && !_miniMapInitialized)
            {
                miniMap.gameObject.SetActive(true);
                _miniMapInitialized = true;
            }
        }

        public void AddObjectiveMarker(ObjectivePosition sender)
        {
            if (miniMap == null)
            {
                miniMap = MinimapController.Instance;
                InitialiseMiniMap();
            }

            RectTransform contentRectTransform = miniMap.contentRectTransform; // Use property or method
            if (contentRectTransform != null)
            {
                if (questIconPrefab != null)
                {
                    RectTransform rectTransform = Instantiate(questIconPrefab, contentRectTransform).GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        currentObjectives.Add((sender, rectTransform));
                    }
                }
            }
        }

        public void RemoveObjectiveMarker(ObjectivePosition sender)
        {
            var objectiveMarker = currentObjectives.Find(objective => objective.objectivePosition == sender);
            if (objectiveMarker != default)
            {
                Destroy(objectiveMarker.markerRectTransform.gameObject);
                currentObjectives.Remove(objectiveMarker);
            }
        }

        public void UpdateObjectiveMarker(ObjectivePosition sender)
        {
            var objectiveMarker = currentObjectives.Find(obj => obj.objectivePosition == sender);
            if (objectiveMarker != default)
            {
                Destroy(objectiveMarker.markerRectTransform.gameObject);
                currentObjectives.Remove(objectiveMarker);

                GameObject prefabToInstantiate = sender.questAreaIconIsActive ? questAreaIconPrefab : questIconPrefab;

                RectTransform contentRectTransform = miniMap.contentRectTransform; // Use property or method
                if (contentRectTransform != null)
                {
                    if (prefabToInstantiate != null)
                    {
                        RectTransform newRectTransform = Instantiate(prefabToInstantiate, contentRectTransform).GetComponent<RectTransform>();
                        if (newRectTransform != null)
                        {
                            currentObjectives.Add((sender, newRectTransform));
                        }
                    }
                }
            }
        }
    }
}