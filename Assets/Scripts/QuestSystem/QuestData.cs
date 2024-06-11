using UnityEngine;

namespace QuestSystem
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest Data")]
    public class QuestData : ScriptableObject
    {
        public string title;
        [field: SerializeField] public string Description { get; private set; }
        [SerializeField] bool isVisible;
        [SerializeField] bool isCompleted;
        [SerializeField] bool isTracked;

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
        
        public void SetVisible() => isVisible = true;

        public void SetInvisible() => isVisible = false;

        public bool CheckQuestCompleted() => isCompleted;

        public void CompleteQuest() => isCompleted = true;

        public void ResetQuest() => isCompleted = false;

        public void TrackQuest() => isTracked = true;

        public void UntrackQuest() => isTracked = false;

        public bool CheckQuestTracked() => isTracked;
    }
}