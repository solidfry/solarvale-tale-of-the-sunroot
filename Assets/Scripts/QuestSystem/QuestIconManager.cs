using UnityEngine;

namespace QuestSystem
{
    public class QuestIconManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerCharacter;
        [SerializeField] private SpriteRenderer questUIIcon;

        private bool _isInTrigger;

        private void Start()
        {
            questUIIcon.enabled = true;
        }

        private void Update()
        {
            if (_isInTrigger)
            {
                questUIIcon.enabled = true;
                QuestIconLockedOnPlayer();
            }
            else
            {
                questUIIcon.enabled = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player") && other.CompareTag("Player"))
            {
                _isInTrigger = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player") && other.CompareTag("Player"))
            {
                _isInTrigger = false;
            }
        }

        private void QuestIconLockedOnPlayer()
        {
            Vector3 directionToFace = playerCharacter.transform.position - questUIIcon.transform.position;
            directionToFace.y = 0; // Keep the direction horizontal

            Quaternion lookRotation = Quaternion.LookRotation(directionToFace);
            questUIIcon.transform.rotation = lookRotation;

            // Add the tilt (assuming you want to tilt it on the z-axis by -45 degrees)
            questUIIcon.transform.Rotate(0, 0, -45);
        }
    }
}
