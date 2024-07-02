using UnityEngine;

namespace QuestSystem.Conditions
{
    [CreateAssetMenu(fileName = "New Count Condition", menuName = "Quest System/Conditions/Count", order = 0)]
    public class QuestConditionCount : QuestConditionBase
    {
        [SerializeField] private int requiredCount = 1;
        [SerializeField] private int currentCount = 0;
        
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
        
        bool ConditionMet() => currentCount >= requiredCount;

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
        
    }
}