using System.Collections;
using Events;
using Player;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    ///  This class is added to the player GameObject or any object to allow for interaction with other GameObjects.
    /// </summary>
    public class Interactor : MonoBehaviour
    {
        [SerializeField] Transform interactionRayOrigin;
        private IInteractable _currentInteractable;
        [SerializeField] LayerMask interactableLayers;
        [SerializeField] Vector3 rayOffset;
        [SerializeField] bool showDebugRay = true;
        [SerializeField] float raycastRadius = 0.5f;
        [SerializeField] bool sendInteractEvent = true;
        [SerializeField] float interactCooldown = 0.1f;
        private Coroutine _interactNullCooldown;
        private bool _isInteracting;
        [SerializeField] bool canInteract = true;

        private void Start()
        {
            if (interactionRayOrigin == null)
                interactionRayOrigin = GetComponentInParent<PlayerManager>().GetPlayerTransform();
        }

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

        private Collider[] _hits = new Collider[1]; // Adjust the size as needed

        private void HandleCast()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(GetRayOrigin(), raycastRadius, _hits, interactableLayers);

            if (hitCount > 0)
            {
                for (int i = 0; i < hitCount; i++)
                {
                    Collider hit = _hits[i];
                    if (hit.TryGetComponent(out IInteractable interactable))
                    {
                        if (_currentInteractable != interactable)
                        {
                            _currentInteractable = interactable;
                            SendInteractEvent();
                            // Debug.Log($"Interactable found: {_currentInteractable}");
                        }
                        return; // Exit after finding the first interactable
                    }
                }
            }
            else
            {
                if (_currentInteractable != null)
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

        private Vector3 GetRayOrigin() => interactionRayOrigin.position;
        
        void ClearInteractable()
        {
            _currentInteractable = null;
            GlobalEvents.OnInteractableFound?.Invoke(_currentInteractable);
        }

        private void OnDrawGizmos()
        {
            if (!showDebugRay) return;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(GetRayOrigin() + rayOffset, raycastRadius);

            if (_currentInteractable != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(GetRayOrigin() + rayOffset, raycastRadius);
            }
        }
    }
}
