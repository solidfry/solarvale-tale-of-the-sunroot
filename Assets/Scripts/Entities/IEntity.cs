using JetBrains.Annotations;

namespace Entities
{
    public interface IEntity
    {
        [CanBeNull] EntityData GetEntityData { get; }
    }
}