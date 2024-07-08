using Behaviour.Pathfinding;
using Entities;
using UnityEngine;
using UnityEngine.Events;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "FindTargetNode", menuName = "Behaviours/Nodes/FindTargetNode")]
    public class FindTargetNode : NodeSo
    {
        private bool hasEntered = false;

        [SerializeField]
        public UnityEvent onFindTarget;
        [SerializeField]
        public UnityEvent onTargetFound;

        public override NodeState Process(BehaviourTreeContext context)
        {
            if (!hasEntered)
            {
                if (context.Creature.onFindTarget != null)
                {
                    onFindTarget = context.Creature.onFindTarget;
                }
            
                if (context.Creature.onTargetFound != null)
                {
                    onTargetFound = context.Creature.onTargetFound;
                }
                hasEntered = true;
            }
            
            var t = context.CurrentTargets;
            t.Clear();
            context.Tree.UpdateCurrentTargets(t);
            
            // Find all colliders within sight range and target layer
            Collider[] colliders = Physics.OverlapSphere(context.Agent.transform.position, context.Creature.CurrentSightRange, context.TargetLayer);
            onFindTarget?.Invoke();

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
                
                onTargetFound?.Invoke();
                nodeState = NodeState.Success;
            }
            else
            {
                context.SetTarget(null);
                context.Creature.IncrementSightRange(context);
                nodeState = NodeState.Failure;
            }

            hasEntered = false;
            return nodeState;
        }

        public override void Reset()
        {
            base.Reset();
            hasEntered = false;
            onFindTarget = new UnityEvent();
            onTargetFound = new UnityEvent();
        }
    }
}
