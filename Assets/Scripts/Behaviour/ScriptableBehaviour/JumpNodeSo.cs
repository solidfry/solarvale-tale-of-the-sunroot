using Behaviour.Pathfinding;
using UnityEngine;

namespace Behaviour.ScriptableBehaviour
{
    [CreateAssetMenu(fileName = "JumpNode", menuName = "Behaviours/Nodes/JumpNode")]
    public class JumpNode : NodeSo
    {
        public float jumpForce = 10f;
        public float jumpCooldown = 1f;

        private float _lastJumpTime;

        public override NodeState Process(BehaviourTreeContext context)
        {
            if (Time.time - _lastJumpTime < jumpCooldown)
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

            // Apply the jump force
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _lastJumpTime = Time.time;

            nodeState = NodeState.Success;
            return nodeState;
        }

        public override void Reset()
        {
            _lastJumpTime = 0f;
        }
    }
    
}