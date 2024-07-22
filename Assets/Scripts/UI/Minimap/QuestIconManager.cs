using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestIconManager : MonoBehaviour
{
    [SerializeField] private GameObject questIconPrefab;
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private RectTransform questIconParentRectTranform;
    [SerializeField] private Camera minimapCamera;

    private List<(ObjectivePosition objectivePosition, RectTransform markerRectTransform)> currentObjectives;

    private void Awake()
    {
        // Initialize the currentObjectives list
        currentObjectives = new List<(ObjectivePosition objectivePosition, RectTransform markerRectTransform)>();
    }

    private void Update()
    {
        foreach ((ObjectivePosition objectivePosition, RectTransform markerRectTransform) marker in currentObjectives)
        {
            // Calculate offset position relative to the player character
            Vector3 offset = marker.objectivePosition.transform.position - playerCharacter.transform.position;

            // Normalize offset based on the camera's orthographic size
            offset /= minimapCamera.orthographicSize;

            // Scale to fit within the minimap parent rect's bounds
            offset *= questIconParentRectTranform.rect.width / 2f;

            // Clamp the offset to fit within a square boundary
            offset.x = Mathf.Clamp(offset.x, -questIconParentRectTranform.rect.width / 2f, questIconParentRectTranform.rect.width / 2f);
            offset.z = Mathf.Clamp(offset.z, -questIconParentRectTranform.rect.width / 2f, questIconParentRectTranform.rect.width / 2f);

            // Set the marker's anchored position
            marker.markerRectTransform.anchoredPosition = new Vector2(offset.x, offset.z);
        }
    }

    // Add a new quest marker
    public void AddObjectiveMarker(ObjectivePosition sender)
    {
        RectTransform rectTransform = Instantiate(questIconPrefab, questIconParentRectTranform).GetComponent<RectTransform>();
        currentObjectives.Add((sender, rectTransform));
    }

    // Remove an existing quest marker
    public void RemoveObjectiveMarker(ObjectivePosition sender)
    {
        if (!currentObjectives.Exists(objective => objective.objectivePosition == sender))
            return;
        (ObjectivePosition pos, RectTransform rectTrans) foundObj = currentObjectives.Find(objective => objective.objectivePosition == sender);
        Destroy(foundObj.rectTrans.gameObject);
        currentObjectives.Remove(foundObj);
    }
}
