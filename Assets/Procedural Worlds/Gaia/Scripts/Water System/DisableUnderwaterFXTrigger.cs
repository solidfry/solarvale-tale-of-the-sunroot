using UnityEngine;

namespace Gaia
{
    public class DisableUnderwaterFXTrigger : MonoBehaviour
    {
        public string m_tagCheck = "Player";

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == m_tagCheck)
            {
                if (GaiaUnderwaterEffects.Instance != null)
                {
                    GaiaUnderwaterEffects.Instance.EnableUnderwaterEffects = false;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == m_tagCheck)
            {
                if (GaiaUnderwaterEffects.Instance != null)
                {
                    GaiaUnderwaterEffects.Instance.EnableUnderwaterEffects = true;
                }
            }
        }

        public void LoadTagFromGaia()
        {
            CharacterController controller = GaiaUtils.FindOOT<CharacterController>();
            if (controller != null)
            {
                m_tagCheck = controller.tag;
            }
        }
    }
}