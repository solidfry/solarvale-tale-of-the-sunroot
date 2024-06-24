using System.Collections.Generic;
using QuestSystem.InitialisationActions;
using UnityEngine;

namespace QuestSystem.Conditions
{
    // [CreateAssetMenu(fileName = "New Quest Condition", menuName = "Quest System/Quest Condition", order = 0)]
    public abstract class QuestConditionBase : ScriptableObject
    {
        [field: SerializeField] public string Title { get; private set; }
        [field: SerializeField] public bool IsComplete { get; private set; }
        
        [SerializeField] List<InitialisationAction> initialisationActions;

        public virtual void OnValidate()
        {
            ResetCondition();
        }

        public virtual void ResetCondition() => IsComplete = false;

        public virtual void UpdateCondition()
        {
           return; 
        }
        
        public virtual void SetConditionComplete() => IsComplete = true;
        public virtual bool IsConditionComplete() => IsComplete;

        public virtual void InitialiseConditionActions()
        {
            if (initialisationActions is null) return;
            foreach (var action in initialisationActions)
            {
                action.Execute();
            }
        }
        
        public virtual void ClearConditionsActions()
        {
            if (initialisationActions is null) return;
            foreach (var action in initialisationActions)
            {
                action.Clear();
            }
        }
    }
}