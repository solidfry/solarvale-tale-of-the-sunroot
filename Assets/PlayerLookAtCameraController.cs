using Cinemachine;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerLookAtCameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera;
    [SerializeField] Transform lookAtTarget;
    [FormerlySerializedAs("originalLookAtPosition")] [SerializeField] Transform resetLookAtPosition;
    [SerializeField] float lerpSpeed = 1f;
    [Range(-1,1)]
    [SerializeField] float dotProductThreshold = 0.1f;
    
    PlayerManager _playerManager;
    Transform _playerTransform;
    
    private void Awake()
    {
        _playerManager = GetComponent<PlayerManager>();
        
        if (_playerManager is not null)
        {
            _playerTransform = _playerManager.GetPlayerTransform();
        }
        
        resetLookAtPosition = lookAtTarget.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerTransform is null) return;

        if (!CheckCameraPlayerDotProduct())
        {
            ResetLookAt();
            return;
        } 
        
        UpdateLookAtPosition();
    }

    private void UpdateLookAtPosition()
    {
        lookAtTarget.position = Vector3.Lerp( lookAtTarget.transform.position, virtualCamera.transform.position, Time.deltaTime);
    }

    private void ResetLookAt()
    {
        lookAtTarget.transform.position = resetLookAtPosition.position;
    }

    private bool CheckCameraPlayerDotProduct() => 
        Vector3.Dot(_playerTransform.forward, virtualCamera.transform.forward) < dotProductThreshold;
}
