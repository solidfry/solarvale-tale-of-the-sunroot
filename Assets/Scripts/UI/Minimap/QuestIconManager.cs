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
        currentObjectives = new List<(ObjectivePosition objectivePosition, RectTransform markerRectTransform)>();
    }

    private void Update()
    {
        foreach ((ObjectivePosition objectivePosition, RectTransform markerRectTransform) marker in currentObjectives)
        {
            Vector3 offset = Vector3.ClampMagnitude(marker.objectivePosition.transform.position - playerCharacter.transform.position, minimapCamera.fieldOfView);
            offset = offset / minimapCamera.fieldOfView * (questIconParentRectTranform.rect.width / 2f);
            marker.markerRectTransform.anchoredPosition = new Vector2(offset.x, offset.z);
        }
    }

    public void AddObjectiveMarker(ObjectivePosition sender)
    {
        RectTransform rectTransform = Instantiate(questIconPrefab, questIconParentRectTranform).GetComponent<RectTransform>();
        currentObjectives.Add((sender, rectTransform));
    }

    public void RemoveObjectiveMarker(ObjectivePosition sender)
    {
        if (!currentObjectives.Exists(objective => objective.objectivePosition == sender))
            return;
        (ObjectivePosition pos, RectTransform rectTrans) foundObj = currentObjectives.Find(objective => objective.objectivePosition == sender);
        Destroy(foundObj.rectTrans.gameObject);
        currentObjectives.Remove(foundObj);
    }
}
