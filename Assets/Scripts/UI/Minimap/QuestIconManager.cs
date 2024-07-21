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
        // Iterate through each objective marker and update its position
        foreach ((ObjectivePosition objectivePosition, RectTransform markerRectTransform) marker in currentObjectives)
        {
            // Calculate offset position relative to the player character
            Vector3 offset = marker.objectivePosition.transform.position - playerCharacter.transform.position;

            // Calculate the maximum allowed offset based on the minimap's dimensions
            float maxOffset = questIconParentRectTranform.rect.width / 2f;

            // Clamp the x and z offsets to create a square boundary
            offset.x = Mathf.Clamp(offset.x, -maxOffset, maxOffset);
            offset.z = Mathf.Clamp(offset.z, -maxOffset, maxOffset);

            // Set the marker's anchored position
            marker.markerRectTransform.anchoredPosition = new Vector2(offset.x, offset.z);
        }
    }

    // Add a new quest marker
    public void AddObjectiveMarker(ObjectivePosition sender)
    {
        // Instantiate the quest icon and get its RectTransform
        RectTransform rectTransform = Instantiate(questIconPrefab, questIconParentRectTranform).GetComponent<RectTransform>();
        // Add the objective and its RectTransform to the list
        currentObjectives.Add((sender, rectTransform));
    }

    // Remove an existing quest marker
    public void RemoveObjectiveMarker(ObjectivePosition sender)
    {
        // Check if the objective exists in the list
        if (!currentObjectives.Exists(objective => objective.objectivePosition == sender))
            return;
        // Find the objective and its RectTransform
        (ObjectivePosition pos, RectTransform rectTrans) foundObj = currentObjectives.Find(objective => objective.objectivePosition == sender);
        // Destroy the quest icon GameObject
        Destroy(foundObj.rectTrans.gameObject);
        // Remove the objective from the list
        currentObjectives.Remove(foundObj);
    }
}
