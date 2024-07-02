using Entities;
using UnityEngine;

namespace QuestSystem.Conditions
{
    [CreateAssetMenu(fileName = "New Photograph Condition", menuName = "Quest System/Conditions/Photograph Condition")]
    public class QuestConditionPhotograph : QuestConditionBase
    {
        [SerializeField] private int requiredCount = 1;
        [SerializeField] private int currentCount = 0;
        
        [SerializeField] private EntityType requiredEntityType;
        [SerializeField] private EntityData requiredEntityData;
        
        public override void ResetCondition()
        {
            base.ResetCondition();
            currentCount = 0;
        }
        
        public override void OnValidate()
        {
            base.OnValidate();
            currentCount = 0;
        }
        
        void IncrementCount()
        {
            currentCount++;
            if (currentCount >= requiredCount)
            {
                SetConditionComplete();
            }
        }
        
        bool ConditionMet()
        {
            // We need to check if the player has taken a photograph of the required entity type
            
            return  currentCount >= requiredCount;
        }

        public override void UpdateCondition()
        {
            if (ConditionMet())
            {
                SetConditionComplete();
                return;
            } 
            IncrementCount();
        }

        public override bool IsConditionComplete()
        {
            if (!ConditionMet()) return false;
            SetConditionComplete();
            return true;
        }
        
        public EntityType GetEntityType()
        {
            return requiredEntityType;
        }

        public EntityData GetEntityData()
        {
            return requiredEntityData;
        }
    }
}