using TMPro;
using UnityEngine;

namespace QuestSystem
{
    public class QuestUIManager : MonoBehaviour
    {
        public QuestManager questManager;
        public TMP_Text questLogText;

        void Start()
        {
            questManager.questLogText = questLogText;
        }
    }
}
