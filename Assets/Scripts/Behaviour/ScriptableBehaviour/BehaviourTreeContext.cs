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
        public Dictionary<int, bool> NodeStates { get; private set; } = new Dictionary<int, bool>();
        public Dictionary<int, int> NodeChildIndices { get; private set; } = new Dictionary<int, int>();
        private Dictionary<int, float> nodeTimers = new Dictionary<int, float>();

    
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
        
        public void SetDesiredLocation(Vector3 location)
        {
            Agent.SetDestination(location);
        }
        
        public bool GetNodeState(int nodeId)
        {
            return NodeStates.TryGetValue(nodeId, out var state) && state;
        }

        public void SetNodeState(int nodeId, bool state)
        {
            NodeStates[nodeId] = state;
        }
        
        public int GetNodeChildIndex(int nodeId)
        {
            return NodeChildIndices.TryGetValue(nodeId, out var index) ? index : 0;
        }

        public void SetNodeChildIndex(int nodeId, int index)
        {
            NodeChildIndices[nodeId] = index;
        }
        
        public float GetNodeTimer(int nodeId)
        {
            return nodeTimers.TryGetValue(nodeId, out var timer) ? timer : 0f;
        }
        
        public void SetNodeTimer(int nodeId, float timer)
        {
            nodeTimers[nodeId] = timer;
        }
        
    }
}