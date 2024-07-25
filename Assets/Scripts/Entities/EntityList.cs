using System.Collections.Generic;
using UnityEngine;

namespace Entities
{
    [CreateAssetMenu(fileName = "New Entity List", menuName = "Entities/Entity List", order = 0)]
    public class EntityList : ScriptableObject
    {
        [SerializeField] private List<EntityData> entities;
        
        public List<EntityData> Entities => entities;
    }
}