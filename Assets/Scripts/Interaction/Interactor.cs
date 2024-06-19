using System.Collections;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

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

        private void FixedUpdate() => CheckInteractable();

        private void CheckInteractable()
        {
            if (Physics.SphereCast(GetRayOrigin(), raycastRadius, transform.forward, out var hit, interactableDistance, interactableLayers))
            {
                if (hit.collider.TryGetComponent(out IInteractable interactable))
                {
                    if (_currentInteractable != interactable)
                    {
                        _currentInteractable = interactable;
                        SendInteractEvent();
                        Debug.Log($"Interactable found: {_currentInteractable}");
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
                    Debug.Log("No Interactable found");
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

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (_currentInteractable == null) return;
            _currentInteractable.Interact();
        }

        private Vector3 GetRayOrigin() => transform.position + rayOffset;

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
