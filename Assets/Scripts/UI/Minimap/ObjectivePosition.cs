using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UI.Minimap
{
    public class ObjectivePosition : MonoBehaviour
    {
        public bool isAreaQuest;
        public bool questAreaIconIsActive;

        private QuestIconManager questIconManager;

        void Start()
        {
            questIconManager = FindObjectOfType<QuestIconManager>();

            if (isAreaQuest)
            {
                AddSphereCollider();
            }
        }

        private void AddSphereCollider()
        {
            SphereCollider sphereCollider = gameObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 150;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isAreaQuest && other.CompareTag("Player") && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.Log("Player entered area quest zone.");
                questAreaIconIsActive = true;
                questIconManager.UpdateObjectiveMarker(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (isAreaQuest && other.CompareTag("Player") && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.Log("Player exited area quest zone.");
                questAreaIconIsActive = false;
                questIconManager.UpdateObjectiveMarker(this);
            }
        }
    }
}