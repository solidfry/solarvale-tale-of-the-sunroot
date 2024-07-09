using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;
using UnityEngine.AI;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "FindRandomPositionNode", menuName = "Behaviours/Nodes/FindRandomPositionNode")]
    public class FindRandomPositionAndMoveTowardsNode : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            if (context.Agent == null)
            {
                Debug.LogError("NavMeshAgent not set in FindRandomPositionNode");
                return NodeState.Failure;
            }

            Debug.Log("Finding random position");

            Vector3 randomDirection = Random.insideUnitSphere * context.Creature.GetStats.SightRange;
            randomDirection += context.Agent.transform.position;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomDirection, out hit, context.Creature.GetStats.SightRange, 1))
            {
                Vector3 finalPosition = hit.position;
                if (context.Agent.isStopped) context.Agent.isStopped = false;
                context.Agent.SetDestination(finalPosition);
                context.SetDesiredLocation(finalPosition);
                Debug.Log("Moving to random position");
                nodeState = NodeState.Success;
            }
            else
            {
                Debug.Log("Failed to find random position");
                nodeState = NodeState.Failure;
            }

            return nodeState;
        }
        
        
        
    }
}