using Entities.Plants;
using Events;
using JetBrains.Annotations;
using UnityEngine;

namespace Entities
{
    public interface IEntity<T> where T : EntityData
    {
        [CanBeNull] T GetEntityData { get; }
    }

    public interface IEdible : IEntity<PlantEntityData>
    {
        void Consume();
        bool IsConsumed { get; }
        void Reset();
        Transform  GetTransform { get; }
    }
    
}