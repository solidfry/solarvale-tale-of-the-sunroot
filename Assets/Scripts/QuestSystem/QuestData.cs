using System.Collections.Generic;
using System.Linq;
using QuestSystem.Conditions;
using QuestSystem.InitialisationActions;
using UnityEngine;
using UnityEngine.Serialization;

namespace QuestSystem
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        // [SerializeField] bool isVisible;
        [SerializeField] bool isCompleted;
        // [SerializeField] bool isTracked;
        
        [Header("Quest Conditions")]
        [SerializeField] List<QuestConditionBase> questConditions;
        
        [Header("Quest Initialisation Actions")]
        [SerializeField] List<InitialisationAction> initialisationActions;
        [SerializeField] bool cleanUpInitialisationActionsOnComplete;
        
        [Header("Quest Chaining")]
        [SerializeField] QuestData nextQuest;
        
        private void Reset()
        {
            ResetQuest();
        }

#if UNITY_EDITOR  
        private void OnValidate()
        {
            ResetQuest();
        }
#endif

        public bool CheckQuestCompleted() => isCompleted;

        public void CompleteQuest()
        {
            if (IsAllQuestConditionsComplete() && !isCompleted) 
                isCompleted = true;

            ClearInitialisedActions();
        }

        private void ClearInitialisedActions()
        {
            if (!isCompleted || !cleanUpInitialisationActionsOnComplete) return;
            questConditions.ForEach(condition =>
            {
                condition.ResetCondition();
                condition.ClearConditionsActions();
            });

            initialisationActions.ForEach(action => action.Clear());
        }

        public void ResetQuest() => isCompleted = false;
        
        
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
        }

        public void InitialiseQuest()
        {
            if (initialisationActions is not null)
            {
                initialisationActions.ForEach(action => action.Execute());
            }
            if (!HasQuestConditions()) return;
            questConditions.ForEach(condition => condition.InitialiseConditionActions());
        }
    }

    
}