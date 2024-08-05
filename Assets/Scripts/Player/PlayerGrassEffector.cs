using UnityEngine;

namespace Player
{
    public class PlayerGrassEffector : MonoBehaviour
    {
        CharacterController _characterController;
        private void Awake()
        { 
            _characterController = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_characterController == null) return;
            Shader.SetGlobalVector("_PlayerPosition", transform.position + Vector3.up *  _characterController.radius);
        }
    }
}
