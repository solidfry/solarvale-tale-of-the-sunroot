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
        [SerializeField] private GameObject[] removeUsedInteractable;
        [SerializeField] private GameObject[] addNewInteractable;

        private void Update()
        {
            CheckQuestCompletion();
        }

        private void CheckQuestCompletion()
        {
            if (questDataToCheck != null && questDataToCheck.CheckQuestCompleted())
            {
                for (int i = 0; i < addNewInteractable.Length; i++)
                {
                    addNewInteractable[i].SetActive(true);
                }
                for (int i=0; i< removeUsedInteractable.Length; i++)
                {
                    removeUsedInteractable[i].SetActive(false);
                }
                Destroy(this);
            }
        }
    }
}
