using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Behaviour.Pathfinding
{
    
    public enum NodeState
    {
        Running,
        Success,
        Failure
    }
    
    public interface IStrategy
    {
        NodeState Process();

        void Reset()
        {
            // Noop
        }
    }
    
    public class PatrolStrategy : IStrategy
    {
        readonly Transform entity;
        readonly NavMeshAgent agent;
        readonly List<Transform> waypoints;
        private readonly float speed;
        private int currentIndex;
        bool isPathCalculated;

        public PatrolStrategy(Transform entity, NavMeshAgent agent, List<Transform> waypoints, float speed = 2f)
        {
            this.entity = entity;
            this.agent = agent;
            this.waypoints = waypoints;
            this.speed = speed;
        }
        
        public NodeState Process()
        {
            if (currentIndex == waypoints.Count) return NodeState.Success;
            var target = waypoints[currentIndex];
            agent.SetDestination(target.position);
            // entity.LookAt(target);
            
            if (isPathCalculated && agent.remainingDistance <= agent.stoppingDistance)
            {
                currentIndex++;
                isPathCalculated = false;
            }

            if (agent.pathPending)
            {
                isPathCalculated = true;
            }
            
            return NodeState.Running;
        }
        
        public void Reset() => currentIndex = 0;
    }

    public class ActionStrategy : IStrategy
    {
        readonly System.Action action;
        
        public ActionStrategy(System.Action action)
        {
            this.action = action;
        }
        
        public NodeState Process()
        {
            action();
            return NodeState.Success;
        }
    }
    
    public class Condition : IStrategy
    {
        readonly System.Func<bool> condition;
        
        public Condition(System.Func<bool> condition)
        {
            this.condition = condition;
        }
        
        public NodeState Process() => condition() ? NodeState.Success : NodeState.Failure;
        
    }
}