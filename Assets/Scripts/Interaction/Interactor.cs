using System.Collections;
using Events;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    ///  This class is added to the player GameObject or any object to allow for interaction with other GameObjects.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        private IInteractable _currentInteractable;
        [SerializeField] LayerMask interactableLayers;
        [SerializeField] Vector3 rayOffset;
        [SerializeField] float interactableDistance = 3f;
        [SerializeField] bool showDebugRay = true;
        [SerializeField] float raycastRadius = 0.5f;
        [SerializeField] bool sendInteractEvent = true;
        [SerializeField] float interactCooldown = 0.1f;
        private Coroutine _interactNullCooldown;
        private bool _isInteracting;
        [SerializeField] bool canInteract = true;

        private void FixedUpdate() => CheckInteractable();

        private void OnEnable()
        {
            GlobalEvents.OnSetCanInteractEvent += SetCanInteract;
        }
        
        private void OnDisable()
        {
            GlobalEvents.OnSetCanInteractEvent -= SetCanInteract;
        }

        private void SetCanInteract(bool value)
        {
            canInteract = value;
            if (value) 
                StartCoroutine(SetInteractableNull());
        }

        private void CheckInteractable()
        {
            if (!canInteract)
            {
                ClearInteractable();
                return;
            }

            HandleCast();
        }

        private void HandleCast()
        {
            if (Physics.SphereCast(GetRayOrigin(), raycastRadius, transform.forward, out var hit, interactableDistance, interactableLayers))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    if (_currentInteractable != interactable)
                    {
                        _currentInteractable = interactable;
                        SendInteractEvent();
                        // Debug.Log($"Interactable found: {_currentInteractable}");
                    }
                }
            }
            else
            {
                if (_currentInteractable != null && hit.collider == null)
                {
                    if (_isInteracting) return;

                    if (_interactNullCooldown != null)
                    {
                        StopCoroutine(_interactNullCooldown);
                    }

                    _interactNullCooldown = StartCoroutine(SetInteractableNull());
                    // Debug.Log("No Interactable found");
                }
            }
        }

        private IEnumerator SetInteractableNull()
        {
            _isInteracting = true;
            yield return new WaitForSeconds(interactCooldown);
            _currentInteractable = null;
            SendInteractEvent();
            _isInteracting = false;
        }

        private void SendInteractEvent()
        {
            if (sendInteractEvent)
                GlobalEvents.OnInteractableFound?.Invoke(_currentInteractable);
        }

        public void OnInteract()
        {
            if (_currentInteractable == null || !canInteract) return;
            _currentInteractable.Interact();
        }

        private Vector3 GetRayOrigin() => transform.position + rayOffset;
        
        void ClearInteractable()
        {
            _currentInteractable = null;
            GlobalEvents.OnInteractableFound?.Invoke(_currentInteractable);
        }

        private void OnDrawGizmos()
        {
            if (!showDebugRay) return;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(GetRayOrigin(), transform.forward * interactableDistance);
            Gizmos.DrawWireSphere(GetRayOrigin() + transform.forward * interactableDistance, raycastRadius);

            if (_currentInteractable != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(GetRayOrigin(), transform.forward * interactableDistance);
                Gizmos.DrawWireSphere(GetRayOrigin() + transform.forward * interactableDistance, raycastRadius);
            }
        }
    }
}
