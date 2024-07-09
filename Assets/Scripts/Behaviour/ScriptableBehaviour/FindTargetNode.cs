using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using Entities;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "FindTargetNode", menuName = "Behaviours/Nodes/FindTargetNode")]
    public class FindTargetNode : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            var t = context.CurrentTargets;
            t.Clear();
            context.Tree.UpdateCurrentTargets(t);
            
            context.Creature.onFindTarget?.Invoke();
            
            // Find all colliders within sight range and target layer
            Collider[] colliders = Physics.OverlapSphere(context.Agent.transform.position, context.Creature.CurrentSightRange, context.TargetLayer);

            foreach (var col in colliders)
            {
                if (col.TryGetComponent(out IEdible edible))
                {
                    if (!edible.IsConsumed && !edible.IsOccupied)
                    {
                        context.AddTarget(edible);
                    }
                }
            }

            // Check if any targets were found
            if (context.CurrentTargets.Count > 0)
            {
                // Set the first target as the current target
                context.SetTarget(context.CurrentTargets[0].GetTransform);
                Debug.Log($"Target found: {context.Target.name}");
                
                context.Creature.onTargetFound?.Invoke();
                nodeState = NodeState.Success;
            }
            else
            {
                context.SetTarget(null);
                context.Creature.IncrementSightRange(context);
                nodeState = NodeState.Failure;
            }
            
            if (nodeState == NodeState.Success)
            {
                context.Creature.ResetSightRange();
            }

            return nodeState;
        }
    }
}