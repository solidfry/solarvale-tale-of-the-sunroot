using UnityEngine;

namespace OccaSoftware.Buto.Runtime
{
    public abstract class ButoPlaceableObject : MonoBehaviour
    {
        protected abstract void Reset();
        protected abstract void OnEnable();
        protected abstract void OnDisable();

        protected virtual float GetSqrMagnitude(Vector3 comparator)
        {
            return Vector3.SqrMagnitude(comparator - transform.position);
        }
    }
}
