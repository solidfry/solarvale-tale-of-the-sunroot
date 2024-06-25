using System.Collections.Generic;
using Entities;
using Events;
using UnityEngine;

public class PhotographyQuestUpdater : MonoBehaviour
{
    [SerializeField] List<EntityData> entitiesPhotographed;

    public void UpdatePhotographyQuests(EntityData eData)
    {   
        entitiesPhotographed.Add(eData);
        GlobalEvents.OnPhotographConditionUpdatedEvent?.Invoke(eData);
        Debug.Log("PhotographyQuestUpdater Sent Event" + eData);
    }

    private void OnDisable()
    {
        entitiesPhotographed.Clear();
    }
}
