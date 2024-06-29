using System.Collections.Generic;
using UnityEngine;

namespace Creatures
{
    public class CreatureManager : MonoBehaviour
    {
        [SerializeField] List<Creature> creatures = new ();
        
        public void AddCreature(Creature creature)
        {
            creatures.Add(creature);
        }
        
        public void RemoveCreature(Creature creature)
        {
            creatures.Remove(creature);
        }
        
        public void RemoveAllCreatures()
        {
            creatures.Clear();
        }

        public List<Creature> GetCreatures()
        {
            return creatures;
        }
    }
}