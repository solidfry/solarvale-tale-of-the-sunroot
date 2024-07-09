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
    
    public class MoveToTarget : IStrategy
    {
        private readonly Transform target;
        private readonly NavMeshAgent agent;
        private readonly float stoppingDistance;
        private readonly float speed;
        private bool isPathCalculated;

        public MoveToTarget(NavMeshAgent agent, Transform target, float stoppingDistance = 0.1f, float speed = 2f)
        {
            this.agent = agent;
            this.target = target;
            this.stoppingDistance = stoppingDistance;
            this.speed = speed;
        }

        public NodeState Process()
        {
            agent.SetDestination(target.position);
            
            if (isPathCalculated && agent.remainingDistance <= agent.stoppingDistance)
            {
                return NodeState.Success;
            }

            if (agent.pathPending)
            {
                isPathCalculated = true;
            }
            
            return NodeState.Running;
        }
    }

    public class MoveAwayFromTarget : IStrategy
    {
        private readonly Transform target;
        private readonly NavMeshAgent agent;
        private readonly float speedMulti;
        private readonly float moveAwayTime;
        private bool isPathCalculated;
        private float timer;

        public MoveAwayFromTarget(NavMeshAgent agent, Transform target, float speedMulti = 2f, float moveAwayTime = 0.5f)
        {
            this.agent = agent;
            this.target = target;
            this.speedMulti = speedMulti;
            this.moveAwayTime = moveAwayTime;
        }
        // Use time to move away from target
        public NodeState Process()
        {
            timer += Time.deltaTime;
            if (timer >= moveAwayTime)
            {
                Reset();
                return NodeState.Success;
            }
            
            agent.SetDestination(agent.transform.position + (agent.transform.position - target.position).normalized * speedMulti);
            
            if (isPathCalculated && agent.remainingDistance <= agent.stoppingDistance)
            {
                return NodeState.Success;
            }

            if (agent.pathPending)
            {
                isPathCalculated = true;
            }
            
            return NodeState.Running;
        }
        
        public void Reset()
        {
            timer = 0;
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
    
    public class DelayActionStrategy : IStrategy
    {
        readonly float delay;
        float timer;
        readonly System.Action action;
        
        public DelayActionStrategy(float delay, System.Action action)
        {
            this.delay = delay;
            this.action = action;
        }
        
        public NodeState Process()
        {
            timer += Time.deltaTime;
            if (timer >= delay)
            {
                action();
                return NodeState.Success;
            }
            return NodeState.Running;
        }
    }
    
    public class DoActionWhileDelayingActionStrategy : IStrategy
    {
        readonly float delay;
        float timer;
        readonly System.Action action;
        private readonly System.Action doWhileDelayingAction;
        
        public DoActionWhileDelayingActionStrategy(float delay, System.Action action = null, System.Action doWhileDelayingAction = null)
        {
            this.delay = delay;
            this.action = action;
            this.doWhileDelayingAction = doWhileDelayingAction;
        }
        
        public NodeState Process()
        {
            if (timer < delay)
            {
                timer += Time.deltaTime;
                doWhileDelayingAction?.Invoke();
                return NodeState.Running;
            }
            
            action?.Invoke();
            Reset();
            return NodeState.Success;
        }
        
        public void Reset() => timer = 0;
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