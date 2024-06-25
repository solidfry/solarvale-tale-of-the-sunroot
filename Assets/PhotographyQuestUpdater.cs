using System.Collections.Generic;
using Entities;
using Events;
using UnityEngine;

public class PhotographyQuestUpdater : MonoBehaviour
{
    [SerializeField] private EntityData lastEntityData;
    
    [SerializeField] List<EntityData> entityDataList;

    public void UpdatePhotographyQuests(EntityData eData)
    {   
        entityDataList.Add(eData);
        GlobalEvents.OnPhotographConditionUpdatedEvent?.Invoke(eData);
        Debug.Log("PhotographyQuestUpdater Sent Event" + eData);
    }
}
