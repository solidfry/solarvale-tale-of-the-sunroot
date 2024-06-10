using UnityEngine;

namespace QuestSystem
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest Data")]
    public class QuestData : ScriptableObject
    {
        public string title;
        [TextArea] 
        public string description;
        public bool isVisible;
        public bool isCompleted;
        public bool isTracked;

#if UNITY_EDITOR  
        private void Reset()
        {
            ResetQuest();
        }
#endif
        
        public void SetVisible()
        {
            isVisible = true;
        }
        
        public void SetInvisible()
        {
            isVisible = false;
        }
        
        public bool CheckQuestCompleted()
        {
            return isCompleted;
        }

        public void CompleteQuest()
        {
            isCompleted = true;
        }
  
        public void ResetQuest()
        {
            isCompleted = false;
        }
        
        public void TrackQuest()
        {
            isTracked = true;
        }
        
        public void UntrackQuest()
        {
            isTracked = false;
        }
    }
}