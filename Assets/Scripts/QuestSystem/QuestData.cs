using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuestSystem
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [SerializeField] bool isVisible;
        [SerializeField] bool isCompleted;
        [SerializeField] bool isTracked;
        
        [SerializeField] List<QuestConditionBase> questConditions;
        [SerializeField] QuestData nextQuest;

#if UNITY_EDITOR  
        private void Reset()
        {
            ResetQuest();
        }
        
        private void OnValidate()
        {
            ResetQuest();
        }
#endif
        
        // public void SetVisible() => isVisible = true;
        //
        // public void SetInvisible() => isVisible = false;

        public bool CheckQuestCompleted() => isCompleted;

        public void CompleteQuest()
        {
            if (IsAllQuestConditionsComplete() && !isCompleted) 
                isCompleted = true;
        }

        public void ResetQuest() => isCompleted = false;

        // public void TrackQuest() => isTracked = true;
        //
        // public void UntrackQuest() => isTracked = false;
        //
        // public bool CheckQuestTracked() => isTracked;
        
        public List<QuestConditionBase> GetQuestConditions() => questConditions;
        
        public bool HasQuestConditions() => questConditions.Count > 0;
        
        public bool IsAllQuestConditionsComplete()
        {
            return questConditions.All(condition => condition.IsConditionComplete()) || !HasQuestConditions();
        }
        
        public QuestData GetNextQuest()
        {
            if (nextQuest is null)
            {
                return null;
            }
            return nextQuest;
            // I want to check for null before returning this value
        }
    }

    
}