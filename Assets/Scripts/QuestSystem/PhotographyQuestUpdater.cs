using System.Collections.Generic;
using Entities;
using Events;
using UnityEngine;

namespace QuestSystem
{
    public class PhotographyQuestUpdater : MonoBehaviour
    {
        [SerializeField] List<EntityData> entitiesPhotographed = new();

        public void UpdatePhotographyQuests(EntityData[] eData)
        {
            entitiesPhotographed.Clear();
            foreach (var data in eData)
            {
                entitiesPhotographed.Add(data);
                GlobalEvents.OnPhotographConditionUpdatedEvent?.Invoke(data);
                Debug.Log("PhotographyQuestUpdater Sent Event" + data);
            }
        }

        private void OnDisable()
        {
            entitiesPhotographed.Clear();
        }
    }
}
