using Behaviour.Tree.Nodes;
using UnityEngine;
using UnityEngine.AI;

namespace Creatures.Behaviours
{
    public class TaskGoToTarget : Node
    {
        private Transform _transform;
        private NavMeshAgent _agent;
        
        public TaskGoToTarget(Transform transform, NavMeshAgent agent)
        {
            _transform = transform;
            _agent = agent;
        }
        
        public override NodeState Evaluate()
        {
            Transform target = (Transform)GetData("target");
            if (target is null)
            {
                State = NodeState.Failure;
                return State;
            }
            
            _agent.SetDestination(target.position);
            
            if (_agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending)
            {
                Debug.Log("TaskGoToTarget: Arrived at target");
                State = NodeState.Success;
                return State;
            }
            
            State = NodeState.Running;
            return State;
        }
    }
}