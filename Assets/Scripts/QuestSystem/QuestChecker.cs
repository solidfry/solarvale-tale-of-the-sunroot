using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Events;
using QuestSystem.Conditions;

namespace QuestSystem
{
    public class QuestChecker : MonoBehaviour
    {
        [SerializeField] private QuestData questDataToCheck;
        [SerializeField] private GameObject currentNPC;
        [SerializeField] private GameObject newNPC;

        private void Update()
        {
            CheckQuestCompletion();
        }

        private void CheckQuestCompletion()
        {
            if (questDataToCheck != null && questDataToCheck.CheckQuestCompleted())
            {
                currentNPC.SetActive(false);
                newNPC.SetActive(true);
            }
        }
    }
}
