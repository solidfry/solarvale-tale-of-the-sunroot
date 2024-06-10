using UnityEngine;

namespace QuestSystem
{
    [System.Serializable]
    public class Quest
    {
        public string title;
        public string description;
        public bool isCompleted;

        public Quest(string title, string description)
        {
            this.title = title;
            this.description = description;
            this.isCompleted = false;
        }

        public void CompleteQuest()
        {
            isCompleted = true;
        }
    }
}

