﻿using Behaviour.Pathfinding;
using Behaviour.ScriptableBehaviour.Base;
using Entities.Creatures.Movement;
using UnityEngine;
using UnityEngine.AI;

namespace Behaviour.ScriptableBehaviour.Nodes
{
    [CreateAssetMenu(fileName = "JumpNode", menuName = "Behaviours/Nodes/JumpNode")]
    public class JumpNode : NodeSo
    {
        public override NodeState Process(BehaviourTreeContext context)
        {
            int nodeId = GetInstanceID();
            float lastJumpTime = context.GetNodeTimer(nodeId);
            var movementType = (JumperMovementDefinition)context.Creature.GetStats.MovementDefinition;
            
            if (Time.time - lastJumpTime < movementType.JumpCooldown)
            {
                return NodeState.Failure;
            }

            if (context.Agent == null)
            {
                Debug.LogError("NavMeshAgent not set in JumpNode");
                return NodeState.Failure;
            }

            Rigidbody rb = context.Creature.GetRigidbody();
            if (rb == null)
            {
                Debug.LogError("Rigidbody not found on agent in JumpNode");
                return NodeState.Failure;
            }

            context.Agent.enabled = false; // Disable the NavMeshAgent to allow manual Rigidbody control
            rb.isKinematic = false; // Allow the Rigidbody to be affected by physics
            
            // Apply the jump force
            rb.AddForce(Vector3.up * movementType.JumpForce, ForceMode.Impulse);

            context.SetNodeTimer(nodeId, Time.time);
            context.SetNodeState(nodeId, true);

            context.Creature.StartCoroutine(ReenableNavMeshAgent(context.Agent, movementType.JumpDuration));

            nodeState = NodeState.Success;
            return nodeState;
        }

        public override void Reset()
        {
            // Reset logic is handled by the context, so no need to do anything here.
        }

        private System.Collections.IEnumerator ReenableNavMeshAgent(NavMeshAgent agent, float delay)
        {
            yield return new WaitForSeconds(delay);
            agent.enabled = true; // Re-enable the NavMeshAgent after the jump is completed
        }
    }
}
