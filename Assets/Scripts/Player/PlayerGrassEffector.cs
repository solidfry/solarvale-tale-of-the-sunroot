using UnityEngine;

namespace Player
{
    public class PlayerGrassEffector : MonoBehaviour
    {
        [SerializeField] float additionalOffset = 0.5f;
        
        CharacterController _characterController;
        private static readonly int PlayerPosition = Shader.PropertyToID("_PlayerPosition");

        private float _offset;
        private void Awake()
        { 
            _characterController = GetComponent<CharacterController>();
        }

        // Update is called once per frame
        void Update()
        {
            if (_characterController == null) return;
            _offset = _characterController.radius + additionalOffset;
            Shader.SetGlobalVector(PlayerPosition, transform.position + Vector3.up * _offset);
        }
    }
}
