using System.Collections.Generic;
using Events;
using UnityEngine;

namespace Entities.Creatures
{
    public class CreatureManager : MonoBehaviour
    {
        [SerializeField] List<Creature> creatures = new ();
        
        private void Awake()
        {
            GlobalEvents.OnRegisterCreatureEvent += AddCreature;
        }

        public void AddCreature(Creature creature)
        {
            if (creatures.Contains(creature)) return;
            creatures.Add(creature);
        }

        public void RemoveCreature(Creature creature) => creatures.Remove(creature);

        public void RemoveAllCreatures()
        {
            creatures.Clear();
        }

        public List<Creature> GetCreatures()
        {
            return creatures;
        }
        
        private void OnDestroy()
        {
            GlobalEvents.OnRegisterCreatureEvent -= AddCreature;
            creatures.Clear();
        }
    }
}