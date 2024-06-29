using JetBrains.Annotations;

namespace Entities
{
    public interface IEntity<T> where T : EntityData
    {
        [CanBeNull] T GetEntityData { get; }
    }   
}