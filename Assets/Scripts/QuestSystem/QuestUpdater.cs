using UnityEngine;

namespace QuestSystem
{
    public class QuestUpdater : MonoBehaviour
    {
        [SerializeField] QuestData questData;
        
        public void CompleteQuest()
        {
            questData.CompleteQuest();
        }
    }
}