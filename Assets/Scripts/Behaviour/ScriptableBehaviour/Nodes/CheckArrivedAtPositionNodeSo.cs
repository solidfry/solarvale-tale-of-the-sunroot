using Behaviour.ScriptableBehaviour.Base;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour.Nodes
{
    [CreateAssetMenu(fileName = "CheckMoveToPosition", menuName = "Behaviours/Nodes/CheckArrivedAtPositionNode")]
    public class CheckArrivedAtPositionNodeSo : ConditionNodeSo
    {
        protected override bool CheckCondition(BehaviourTreeContext context)
        {
            if (context.Agent == null)
            {
                Debug.LogError("NavMeshAgent not set in MoveToTargetNode");
                return false;
            }

            if (context.Agent.pathPending)
            {
                return false;
            }
            else if (context.Agent.remainingDistance <= context.Agent.stoppingDistance)
            {
                if (!context.Agent.hasPath || context.Agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}