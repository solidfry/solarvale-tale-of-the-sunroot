using System.Collections.Generic;
using Entities;
using Entities.Creatures;
using UnityEngine;
using UnityEngine.AI;

namespace Behaviour.ScriptableBehaviour
{
    [System.Serializable]
    public class BehaviourTreeContext
    {
        public CreatureScriptableBehaviourTree Tree;
        public NavMeshAgent Agent;
        public Transform Target;
        public Creature Creature;
        public List<IEdible> CurrentTargets;
        public Transform Enemy;
        public LayerMask TargetLayer;
    
        public BehaviourTreeContext(CreatureScriptableBehaviourTree tree,NavMeshAgent agent, Transform target, Creature creature, List<IEdible> currentTargets, Transform enemy, LayerMask targetLayer)
        {
            Tree = tree;
            Agent = agent;
            Target = target;
            Creature = creature;
            CurrentTargets = currentTargets;
            Enemy = enemy;
            TargetLayer = targetLayer;
        }
        
        public void AddTarget(IEdible target)
        {
            CurrentTargets.Add(target);
            Tree.UpdateCurrentTargets(CurrentTargets);
        }
        
        public void UpdateCurrentTargets(List<IEdible> targets)
        {
            CurrentTargets = targets;
            Tree.UpdateCurrentTargets(targets);
        }
        
        public void SetTarget(Transform target)
        {
            Target = target;
            Tree.SetTarget(target);
        }
        
    }
}